Shader "Custom/DynamicWater"
{
    Properties
    {
        _WaterColor ("Water Color", Color) = (0.2, 0.6, 0.9, 0.7)
        _DeepWaterColor ("Deep Water Color", Color) = (0.1, 0.3, 0.6, 0.9)
        _WaveAmplitude ("Wave Amplitude", Range(0, 2)) = 0.3
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2.0
        _DomainWarp ("Domain Warp", Range(0, 2)) = 0.5
        _RefractionStrength ("Refraction Strength", Range(0, 0.5)) = 0.1
        _CustomTime ("Custom Time", Float) = 0
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
                float3 worldPos : TEXCOORD1;
                float depth : TEXCOORD2;
            };

            float4 _WaterColor;
            float4 _DeepWaterColor;
            float _WaveAmplitude;
            float _NoiseScale;
            float _DomainWarp;
            float _RefractionStrength;
            float _CustomTime;

            // Perlin-like noise function
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f); // Smoothstep
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Layered noise with domain warping
            float layeredNoise(float2 p)
            {
                // First layer of noise
                float n = noise(p);
                
                // Domain warp - use noise to distort the position
                float2 q = float2(
                    noise(p + float2(0.0, 0.0)),
                    noise(p + float2(5.2, 1.3))
                );
                
                // Second layer with warped domain
                float2 warpedP = p + q * _DomainWarp;
                
                // Multiple octaves
                float value = 0.0;
                float amplitude = 1.0;
                float frequency = 1.0;
                
                for(int i = 0; i < 4; i++)
                {
                    value += noise(warpedP * frequency) * amplitude;
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                // Only displace vertices on the surface (top vertices)
                if(v.uv.y > 0.5)
                {
                    // Calculate wave displacement
                    float2 noisePos = float2(v.vertex.x * _NoiseScale + _CustomTime * 0.5, _CustomTime * 0.3);
                    float wave = layeredNoise(noisePos);
                    
                    // Add another layer moving in different direction
                    float2 noisePos2 = float2(v.vertex.x * _NoiseScale * 0.7 - _CustomTime * 0.3, _CustomTime * 0.5);
                    wave += layeredNoise(noisePos2) * 0.5;
                    
                    // Normalize and apply amplitude
                    wave = (wave - 0.75) * _WaveAmplitude;
                    
                    v.vertex.y += wave;
                }
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.depth = v.uv.y; // Store depth for color gradient
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate color based on depth
                fixed4 col = lerp(_WaterColor, _DeepWaterColor, 1.0 - i.depth);
                
                // Add some shimmer/highlights near the surface
                if(i.depth > 0.985)
                {
                    float shimmer = noise(i.worldPos.xz * 10.0 + _CustomTime * 2.0);
                    col.rgb += shimmer * 0.1;
                }
                
                // Add refraction-like distortion to color
                float2 distortion = float2(
                    noise(i.worldPos.xz * 5.0 + _CustomTime),
                    noise(i.worldPos.xz * 5.0 + _CustomTime + 10.0)
                );
                distortion = (distortion - 0.5) * _RefractionStrength;
                
                // Modify color slightly based on distortion
                col.rgb += distortion.x * 0.1;
                
                return col;
            }
            ENDCG
        }
    }
}