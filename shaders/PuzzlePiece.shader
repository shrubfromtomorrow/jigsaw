Shader "Alduris/PuzzlePiece"
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
            //GrabPass {}
            Pass 
            {
                CGPROGRAM
                #pragma target 3.0
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "_ShaderFix.cginc"
                #include "_Random.cginc"

                float4 _MainTex_ST;
                sampler2D _MainTex;
                
                sampler2D _PuzzleGrab;
                uniform float2 _PuzzleSize;
                uniform float4 _PuzzleSeed;
                
                uniform float2 _screenSize;
                
                //sampler2D _GrabTexture;

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





                struct ToothData {
                    float pos;
                    float size;
                    float flat;
                    int dir;
                };

                struct PieceData {
                    ToothData leftTooth;
                    ToothData rightTooth;
                    ToothData topTooth;
                    ToothData bottomTooth;
                };

                ToothData getToothDataFor(int x, int y, int horiz) {
                    ToothData o;
                    // Scalars drawn by random number generator
                    o.pos  =  random(float3(0.2347 * x, 2.5487 * y, 4.1219 * horiz) + _PuzzleSeed.xyz);
                    o.size =  random(float3(4.8514 * x, 0.5573 * y, 1.2378 * horiz) + _PuzzleSeed.yzw);
                    o.flat =  random(float3(3.7742 * x, 1.0579 * y, 2.2018 * horiz) + _PuzzleSeed.zwx);
                    o.dir  = (random(float3(7.5384 * x, 3.5627 * y, 6.5849 * horiz) + _PuzzleSeed.xwy) > 0.5) * 2 - 1;
                    return o;
                }

                PieceData getDataFor(int x, int y) {
                    PieceData d;
                    d.leftTooth = getToothDataFor(x-1, y, 1);
                    d.rightTooth = getToothDataFor(x, y, 1);
                    d.topTooth = getToothDataFor(x, y, 0);
                    d.bottomTooth = getToothDataFor(x, y-1, 0);
                    return d;
                }


                static const float SQRT2 = 1.4142135;
                static const float SQRT2_2 = 0.70710678;
                uint insideToothHoriz(uint doGrab, ToothData tooth, float2 uv, int farSide) {
                    float flat = lerp(1, 1.5, tooth.flat);
                    float2 checkPos = float2((uv.x - farSide) * tooth.dir * flat, uv.y);
                    float scale = lerp(1.0/4.0, 1.0/12.0, tooth.size) * 0.5; // 0.5 to convert to radius
                    float baseY = lerp(0.3125, 0.6875, tooth.pos);

                    uint intersecting = checkPos.x > 0;

                    float cBigCheckStartX = (SQRT2_2 + 1.0) * (0.75 * scale);
                    if (checkPos.x > cBigCheckStartX) {
                        // Big round tip
                        float cBigX = cBigCheckStartX + SQRT2_2 * scale;

                        intersecting &= distance(checkPos, float2(cBigX, baseY)) < scale;
                    } else {
                        // Cutoff on sides
                        float cSmallRad = scale * 0.75;
                        float cSmallOffsetY = SQRT2_2 * scale + SQRT2_2 * cSmallRad;
                        float2 smallOffsetUV = float2(checkPos.x, checkPos.y - baseY);

                        intersecting &= abs(smallOffsetUV.y) < cSmallOffsetY;
                        intersecting &= distance(smallOffsetUV, float2(cSmallRad, cSmallOffsetY)) > cSmallRad;
                        intersecting &= distance(smallOffsetUV, float2(cSmallRad, -cSmallOffsetY)) > cSmallRad;
                    }

                    return doGrab != intersecting;
                }

                uint insideToothVerti(uint doGrab, ToothData tooth, float2 uv, int farSide) {
                    return insideToothHoriz(doGrab, tooth, float2(uv.y, uv.x), farSide);
                }


                half4 frag (v2f i) : SV_Target
                {
                    // Coords
                    int x = (int)(i.clr.r * 255);
                    int y = (int)(i.clr.g * 255);
                    int w = (int)_PuzzleSize.x;
                    int h = (int)_PuzzleSize.y;

                    float2 uv = i.uv.xy * 2.0 - 0.5;
                    half2 grabPos = (uv + half2(x, y)) / half2(w, h);

                    // Calculate teeth intersection
                    PieceData teethData = getDataFor(x, y);
                    uint doGrab = uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1;

                    if (x != 0) doGrab = insideToothHoriz(doGrab, teethData.leftTooth, uv, 0);
                    if (x != w - 1) doGrab = insideToothHoriz(doGrab, teethData.rightTooth, uv, 1);
                    if (y != 0) doGrab = insideToothVerti(doGrab, teethData.bottomTooth, uv, 0);
                    if (y != h - 1) doGrab = insideToothVerti(doGrab, teethData.topTooth, uv, 1);

                    /*return half4(
                        insideToothHoriz(doGrab, teethData.leftTooth, uv, 0) + insideToothHoriz(doGrab, teethData.rightTooth, uv, 1),
                        insideToothVerti(doGrab, teethData.bottomTooth, uv, 0) + insideToothVerti(doGrab, teethData.topTooth, uv, 1),
                        doGrab,
                        1);*/

                    // Figure out if we need to color, and grab the color if so
                    if (!doGrab) discard;
                    return lerp(tex2D(_PuzzleGrab, grabPos), half4(1, 1, 1, 1), i.clr.b);
                    //return tex2D(_GrabTexture, grabPos); // temporary
                }
                ENDCG
            }
        } 
    }
}
