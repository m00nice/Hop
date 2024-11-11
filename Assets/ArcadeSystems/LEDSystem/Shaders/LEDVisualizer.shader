// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "LED/Visualizer" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _LEDMask("LED Mask (RGB)", 2D) = "white" {}
    _Overglow("OverGlow", Range(0,3)) = 1
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex, _LEDMask;
            float4 _MainTex_ST, _LEDMask_ST;
            float _Overglow;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //mousePositionInWorld.x = Mathf.Round(mousePositionInWorld.x / GameController.instance.gridSize) * GameController.instance.gridSize;
                //mousePositionInWorld.y = Mathf.Round(mousePositionInWorld.y / GameController.instance.gridSize) * GameController.instance.gridSize;
                float2 uvsClamped = i.texcoord.xy;
                float2 uvsGrid = float2(1.0 / 192.0, 1.0 / 64);
                uvsClamped.x = floor(i.texcoord.x / uvsGrid.x) * uvsGrid.x + uvsGrid.x * .5;
                uvsClamped.y = floor(i.texcoord.y / uvsGrid.y) * uvsGrid.y + uvsGrid.y * .5;

                fixed4 col = tex2D(_MainTex, uvsClamped);
                fixed4 mask = tex2D(_LEDMask, i.texcoord);
                col.rgb *= mask.rgb + mask.rgb * _Overglow * mask.a;
                float colIntensity = (0.222 * col.r + 0.666 * col.g + 0.111 * col.b);
                float intensity = (0.222 * mask.r + 0.666 * mask.g + 0.111 * mask.b);
                col.rgb += col.rgb * .1 * mask.rgb * intensity * intensity;

                float colIntInv = clamp(1.0 - colIntensity, 0.0, 1.0);
                col.rgb += mask.rgb * pow(colIntInv, 8) * .15;
                col.rgb *= 1.15f;
                UNITY_OPAQUE_ALPHA(col.a);
                return col;
            }
        ENDCG
    }
}

}
