Shader "Unlit/BlueprintShader"
{
    Properties
    {
        _WireframeColor("Wireframe color", color) = (1.0, 1.0, 1.0, 1.0)
        _WireframeWidth("Wireframe width", float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back




        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // We add our barycentric variables to the geometry struct.
            struct g2f {
                float4 pos : SV_POSITION;
                float3 barycentric : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // This applies the barycentric coordinates to each vertex in a triangle.
            [maxvertexcount(3)]
            void geom(triangle v2f IN[3], inout TriangleStream<g2f> triStream) {
                g2f o;
                o.pos = IN[0].vertex;
                o.barycentric = float3(1.0, 0.0, 0.0);
                triStream.Append(o);
                o.pos = IN[1].vertex;
                o.barycentric = float3(0.0, 1.0, 0.0);
                triStream.Append(o);
                o.pos = IN[2].vertex;
                o.barycentric = float3(0.0, 0.0, 1.0);
                triStream.Append(o);
            }

            float random (float2 uv)
            {
                return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
            }

            fixed4 _WireframeColor;
            float _WireframeWidth;

            fixed4 frag(g2f i) : SV_Target
            {
                // Calculate the unit width based on triangle size.
                float3 unitWidth = fwidth(i.barycentric);
                // Find the barycentric coordinate closest to the edge.
                float3 edge = step(unitWidth * _WireframeWidth, i.barycentric);
                // Set alpha to 1 if within edge width, else 0.
                float alpha = 1 - min(edge.x, min(edge.y, edge.z));
                if(random(i.pos)>0.75)alpha = 0;
                // Set to our backwards facing wireframe colour.
                return fixed4(_WireframeColor.r, _WireframeColor.g, _WireframeColor.b, alpha);
            }
            ENDCG
        }
    }
}