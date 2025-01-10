Shader "Hidden/MRBlend"
{
    Properties
    {
        _MainTex ("Scene Texture", 2D) = "black" {}
        _CameraTex ("Camera Texture", 2D) = "white" {}
        _SceneScale ("Scene Scale", Vector) = (0.25, 0.25, 0, 0) 
    }
    
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _CameraTex;
            float2 _SceneScale; 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // カメラテクスチャは通常通りサンプリング
                fixed4 camera = tex2D(_CameraTex, i.uv);

                // シーンテクスチャのUVを中心基準に調整
                float2 sceneUV = (i.uv - 0.5) / _SceneScale + 0.5;
                
                // UVが0-1の範囲内かチェック
                bool inBounds = all(sceneUV >= 0 && sceneUV <= 1);
                
                fixed4 scene;
                float mask;
                
                if (inBounds)
                {
                    scene = tex2D(_MainTex, sceneUV);
                    mask = scene.a;
                }
                else
                {
                    // 範囲外の場合はカメラ画像のみ
                    scene = fixed4(0, 0, 0, 0);
                    mask = 0.0;
                }
                
                return lerp(camera, scene, mask);
            }
            ENDCG
        }
    }
}