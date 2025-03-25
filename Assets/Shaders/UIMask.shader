Shader "UI/Custom/UIMask"
{
    Properties
    {
        // スプライトテクスチャのプロパティ（デフォルトは白テクスチャ）
        _MainTex ("Sprite Texture", 2D) = "white" {}
    }
    SubShader
    {
        // レンダリング順序やその他のタグを設定
        Tags
        {
            "Queue" = "Transparent-1" // 他のUI要素より前に描画
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Sprite"
            "CanUseSpriteAtlas" = "True"
        }
        Cull Off              // 背面カリングを無効化
        Lighting Off          // ライティング無効
        ZWrite Off            // Zバッファ書き込み無効
        Blend One Zero        // ブレンド設定（出力色は使用しないため）

        // ステンシル設定：本パスではRef値1を常に書き込み、描画対象領域をマークする
        Stencil
        {
            Ref 1           // 参照値1を設定
            Comp Always     // 常に書き込む
            Pass Replace    // 書き込み時は既存値を置換
        }
        // 色の書き込みを抑制（描画色は不要）
        ColorMask 0

        Pass
        {
            // 必要なライブラリのインクルード
            CGPROGRAM
            // 頂点シェーダーおよびフラグメントシェーダーのエントリーポイントを指定
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // 頂点属性構造体
            struct appdata_t
            {
                float4 vertex : POSITION;    // 頂点位置
                float4 color  : COLOR;       // 頂点カラー
                float2 texcoord : TEXCOORD0;  // テクスチャUV
            };

            // 頂点シェーダーからフラグメントシェーダーへ受け渡す構造体
            struct v2f
            {
                float4 vertex : SV_POSITION; // クリップ空間での頂点位置
                fixed4 color  : COLOR;       // 補助的なカラー情報
                half2 texcoord : TEXCOORD0;  // テクスチャUV
            };

            // 頂点シェーダー
            v2f vert(appdata_t v)
            {
                v2f o;
                // オブジェクト空間の頂点位置をクリップ空間へ変換
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord = v.texcoord;
                return o;
            }

            // フラグメントシェーダー
            fixed4 frag(v2f i) : SV_Target
            {
                // 本パスでは描画色を出力しない（ColorMask 0のため画面には反映されず、ステンシルのみが書き込まれる）
                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
    // 代替シェーダー（該当する場合）
    FallBack "UI/Default"
}
