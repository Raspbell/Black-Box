Shader "Universal Render Pipeline/Custom/UVScrollTriplanarEmission_HDR"
{
    Properties
    {
        // メインテクスチャのプロパティ（デフォルトは白テクスチャ）
        _MainTex ("Texture", 2D) = "white" {}
        // UVスクロールの速度を指定するプロパティ（xy成分を使用）
        _ScrollSpeed ("Scroll Speed", Vector) = (1, 1, 0, 0)
        // テクスチャのタイリング倍率（任意に調整可能）
        _Tiling ("Tiling", Float) = 1.0
        // エミッションカラーのプロパティ（エミッションの強度・色を指定、HDR対応）
        [HDR]_EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        // エミッションテクスチャのプロパティ（デフォルトは黒テクスチャ）
        _EmissionTex ("Emission Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            // 頂点シェーダーおよびフラグメントシェーダーのエントリポイントを指定
            #pragma vertex vert
            #pragma fragment frag

            // URP用のコアライブラリをインクルード（UnityCG.cgincは使用しない）
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 頂点属性構造体
            struct Attributes
            {
                float4 positionOS : POSITION; // オブジェクト空間での頂点位置
                float3 normalOS   : NORMAL;   // オブジェクト空間での法線
            };

            // 頂点シェーダーからフラグメントシェーダーへ受け渡すデータ構造体
            struct Varyings
            {
                float4 positionHCS : SV_POSITION; // クリップ空間での頂点位置
                float3 worldPos    : TEXCOORD0;    // ワールド空間での頂点位置
                float3 worldNormal : TEXCOORD1;    // ワールド空間での法線
            };

            // マテリアルごとの定数バッファ（プロパティのバインド）
            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _ScrollSpeed;
                float _Tiling;
                TEXTURE2D(_EmissionTex);
                SAMPLER(sampler_EmissionTex);
                float4 _EmissionColor;
            CBUFFER_END

            // 頂点シェーダー
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // オブジェクト空間からクリップ空間へ変換
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                // オブジェクト空間からワールド空間へ変換
                OUT.worldPos = TransformObjectToWorld(IN.positionOS).xyz;
                // 法線をワールド空間に変換（正規化を実施）
                OUT.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, IN.normalOS));
                return OUT;
            }

            // トライプラナー・マッピング関数
            // 第1引数：サンプリング対象のテクスチャ
            // 第2引数：対象テクスチャに対するサンプラー
            // 第3,4引数：ワールド空間の位置および法線
            // 第5引数：UVスクロールのオフセット
            float4 TriplanarSample(TEXTURE2D(tex), SAMPLER(samplerTex), float3 worldPos, float3 worldNormal, float2 offset)
            {
                // 各軸の法線成分の絶対値からブレンディングウェイトを算出
                float3 blending = abs(worldNormal);
                blending = normalize(max(blending, 0.00001));
                float total = blending.x + blending.y + blending.z;
                blending /= total;

                // 各軸に対するUV座標を計算（タイリング倍率およびスクロールオフセットを適用）
                float2 uv_x = worldPos.yz * _Tiling + offset; // X軸方向（X軸に垂直な面用）
                float2 uv_y = worldPos.xz * _Tiling + offset; // Y軸方向（Y軸に垂直な面用）
                float2 uv_z = worldPos.xy * _Tiling + offset; // Z軸方向（Z軸に垂直な面用）

                // 各方向でテクスチャをサンプリング
                float4 tex_x = SAMPLE_TEXTURE2D(tex, samplerTex, uv_x);
                float4 tex_y = SAMPLE_TEXTURE2D(tex, samplerTex, uv_y);
                float4 tex_z = SAMPLE_TEXTURE2D(tex, samplerTex, uv_z);

                // ブレンディングウェイトに基づいて各方向のサンプル結果を合成
                return tex_x * blending.x + tex_y * blending.y + tex_z * blending.z;
            }

            // フラグメントシェーダー（HDR対応のため、戻り値をfloat4に変更）
            float4 frag(Varyings IN) : SV_Target
            {
                // 経過時間に応じたUVスクロールのオフセットを計算
                float timeValue = _Time.y;
                float2 offset = _ScrollSpeed.xy * timeValue;

                // メインテクスチャのトライプラナーサンプリング
                float4 baseColor = TriplanarSample(_MainTex, sampler_MainTex, IN.worldPos, normalize(IN.worldNormal), offset);
                // エミッションテクスチャのトライプラナーサンプリング
                float4 emissionSample = TriplanarSample(_EmissionTex, sampler_EmissionTex, IN.worldPos, normalize(IN.worldNormal), offset);
                // エミッションカラーと乗算してエミッションを算出（HDR対応の値をそのまま反映）
                float4 emission = emissionSample * _EmissionColor;
                // 最終出力色として、メインテクスチャの色にエミッションを加算（アルファはメインテクスチャの値をそのまま使用）
                float4 finalColor = baseColor + emission;
                return float4(finalColor.rgb, baseColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}
