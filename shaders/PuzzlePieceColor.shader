Shader "Alduris/PuzzlePieceColor"
{
    Properties 
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }
    
    Category 
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        SubShader
        {
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


                half4 frag (v2f i) : SV_Target
                {

                    float2 uv = i.uv.xy * 2.0 - 0.5;

                    // We want all the teeth to be the same color for basicness and stuff
                    ToothData toothData;
                    toothData.pos = 0.5;
                    toothData.size = 0.5;
                    toothData.flat = 0.333;
                    toothData.dir = 1;
                    uint doGrab = uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1;

                    doGrab = insideToothHoriz(doGrab, toothData, uv, 0);
                    doGrab = insideToothHoriz(doGrab, toothData, uv, 1);
                    doGrab = insideToothVerti(doGrab, toothData, uv, 0);
                    doGrab = insideToothVerti(doGrab, toothData, uv, 1);

                    if (!doGrab) discard;
                    return i.clr;
                }
                ENDCG
            }
        } 
    }
}
