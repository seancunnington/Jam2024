Shader "FishGame/shad_Fish"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        [Header(Test Sway Effects)][Space(5)]
        _TestSwaySpeed ("Swim Sway Speed", Range(2, 50)) = 5
        _TestZOffset ("Z Offset Mask", Float) = 2.1                 // how far from the front of the fish to mask the swaying
        _TestYaw ("Yaw", Float) = 1.5
        _TestRoll ("Roll", Float) = 1.5
        _TestScale ("Scale", Float)= 0.2

        [Header(Lighting)][Space(5)]
        _Ambient ("Ambient Light Strength", Range(0, 1)) = 0

        [Header(Debug)][Space(5)]
        [Toggle(TEST_SWIM_VALUES)] _TEST_SWIM ("Use shader parameters instead of instance parameters?", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf ToonRamp vertex:vert fullforwardshadows addshadow
        #pragma shader_feature TEST_SWIM_VALUES
      

        #define RAMP_SMOOTHNESS     0.02
        #define OFFSET              0
        #define OFFSET_POINT        0.5
        float _Ambient;

        // Custom Toon Lighting function
        inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
        {
            #ifndef USING_DIRECTIONAL_LIGHT
                lightDir = normalize(lightDir);
            #endif

            float d = dot(s.Normal, lightDir);
            float lightIntensity = smoothstep(OFFSET, OFFSET + RAMP_SMOOTHNESS, d);
            
            #if POINT || SPOT 
                lightIntensity = smoothstep(OFFSET_POINT, OFFSET_POINT + RAMP_SMOOTHNESS, d);
            #endif

            lightIntensity += _Ambient;
            lightIntensity += step(d, atten);

            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * lightIntensity;
            c.a = s.Alpha;
            return c;
        }


        // Other Variables
        sampler2D _MainTex;
        float4 _Color;
        
        float _TestSwaySpeed;   // 2 - 50
        float _TestZOffset;     // 0.05
        float _TestYaw;         // 40
        float _TestRoll;        // 40 
        float _TestScale;       // 0.02

        #define MASK_FEATHER    10

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;   // hacky way of getting vertex position data to surf
        };


        // Instancing
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _Instance_Time)
            UNITY_DEFINE_INSTANCED_PROP(float, _ZOffset)
            UNITY_DEFINE_INSTANCED_PROP(float, _Yaw)
            UNITY_DEFINE_INSTANCED_PROP(float, _Roll)
            UNITY_DEFINE_INSTANCED_PROP(float, _Scale)
        UNITY_INSTANCING_BUFFER_END(Props)


        // Vertex Shader
        void vert (inout appdata_full v)
        {
            // Set the vertex position BEFORE appling vert offset effects - makes specular mapping move with these effects too
            v.color = float4(v.vertex.xyz, 0);

            // Sway Effects
            float zOffset, time;
            float yaw, roll, scale;

            #if !TEST_SWIM_VALUES
                zOffset = _ZOffset;
                time = _Instance_Time; // time is separated so CPU can have an animated time for increasing and decreasing swim speeds.
                yaw = _Yaw;
                roll = _Roll;
                scale = _Scale;
            #else 
                zOffset = _TestZOffset;
                time = _Time.y * _TestSwaySpeed;
                yaw = _TestYaw;
                roll = _TestRoll;
                scale = _TestScale;
            #endif


            float mask = clamp((zOffset - v.vertex.z) * MASK_FEATHER, 0, 1);

            // Calculate vertex animation
            float3 vertexPosition = v.vertex.xyz;
            vertexPosition +=   ((sin(((time)
                                + (vertexPosition.z * yaw * mask)
                                + (vertexPosition.y * roll * mask))) * scale * mask)
                                * float3(1, 0, 0));

            // Pass vertex animation
            v.vertex.xyz = vertexPosition;
        }

        // Surface Shader
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Main Texture
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * fixed4(_Color.rgb, 1);

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
