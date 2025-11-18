Shader "Alduris/PuzzlePieceOutline"
{
    Properties 
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }
    
    Category 
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend One Zero
        Cull Off

        SubShader
        {
            GrabPass {}
            Pass 
            {
                CGPROGRAM
                #pragma target 3.0
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "_ShaderFix.cginc"
                #include "_Puzzle.cginc"

                float4 _MainTex_ST;
                sampler2D _MainTex;
                
                //sampler2D _PuzzleGrab;
                
                sampler2D _GrabTexture;
                uniform float2 _screenSize;

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float2 scrPos : TEXCOORD1;
                    float4 clr : COLOR;
                };

                v2f vert (appdata_full v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.scrPos = ComputeScreenPos(o.pos);
                    o.clr = v.color;
                    return o;
                }

                uint insideTooth(PieceData teethData, float2 uv, int4 tile) {
                    uint doGrab = uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1;

                    if (tile.x != 0) doGrab = insideToothHoriz(doGrab, teethData.leftTooth, uv, 0);
                    if (tile.x != tile.z - 1) doGrab = insideToothHoriz(doGrab, teethData.rightTooth, uv, 1);
                    if (tile.y != 0) doGrab = insideToothVerti(doGrab, teethData.bottomTooth, uv, 0);
                    if (tile.y != tile.w - 1) doGrab = insideToothVerti(doGrab, teethData.topTooth, uv, 1);

                    return doGrab;
                }


                half4 frag (v2f i) : SV_Target
                {
                    // Coords
                    int x = (int)(i.clr.r * 255);
                    int y = (int)(i.clr.g * 255);
                    int w = (int)_PuzzleSize.x;
                    int h = (int)_PuzzleSize.y;

                    float2 uv = i.uv.xy * 2.0 - 0.5;

                    float2 step = _PuzzleSize / _screenSize;
                    //half2 grabPos = (uv + half2(x, y)) / half2(w, h);
                    //grabPos = (floor(grabPos * _ScreenParams.xy) + 0.5) / _ScreenParams.xy;

                    // Calculate teeth intersection
                    PieceData teethData = getDataFor(x, y);
                    int4 tile = int4(x, y, w, h);
                    uint origIntersect = insideTooth(teethData, uv, tile);
                    uint doGrab = 0;
                    for (int a = -2; a <= 2; a++) {
                        for (int b = -2; b <= 2; b++) {
                            doGrab |= (insideTooth(teethData, uv + step * float2(a, b), tile) != origIntersect) && (distance(float2(a,b), float2(0,0)) < 2);
                        }
                    }

                    // Figure out if we need to color, and grab the color if so
                    if (!doGrab) discard;
                    //return lerp(tex2D(_PuzzleGrab, grabPos), half4(1, 1, 1, 1), i.clr.b);
                    return lerp(tex2D(_GrabTexture, i.scrPos.xy), half4(1, 1, 1, 1), i.clr.b);
                }
                ENDCG
            }
        } 
    }
}
