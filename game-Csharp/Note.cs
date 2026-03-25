using UnityEngine;

public class Note : MonoBehaviour {
    private float targetTime;
    private float scrollSpeed;
    private AudioSource bgm;

    public void Init(int lane, int width, float targetTime, float speed, AudioSource audio) {
        this.targetTime = targetTime;
        this.scrollSpeed = speed;
        this.bgm = audio;

        // 【2D計算】X座標：12レーンの中央を0にする（1レーン幅を0.8ピクセルと想定）
        float posX = (lane - 5.5f) * 0.8f; 
        
        // 初期の高さ(Y)は適当でOK（Updateで上書きされるため）
        transform.position = new Vector2(posX, 10f); 
        
        // 2D Spriteの幅を width に合わせる
        transform.localScale = new Vector3(width * 0.8f, 0.5f, 1f);
    }

    void Update() {
        if (bgm == null) return;

        // 曲の時間を基準に「判定ライン(Y=0)までの距離」を計算
        float timeRemaining = targetTime - bgm.time;

        // Y座標 = 残り時間 × スピード
        // timeRemainingが0のとき、ちょうど Y=0 に重なる
        float posY = timeRemaining * scrollSpeed;
        
        transform.position = new Vector2(transform.position.x, posY);

        // 判定ラインを通り過ぎて0.5秒後に消去
        if (timeRemaining < -0.5f) {
            Destroy(gameObject);
        }
    }
}
