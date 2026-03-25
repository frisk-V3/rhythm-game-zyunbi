using UnityEngine;

public class Note : MonoBehaviour {
    private float targetTime; // 判定ラインに重なるべき時間
    private float spawnTime;  // 生成された時間
    private float scrollSpeed; // 落ちる速度
    private int lane;

    // --- ChartManagerから呼ばれる「初期化（メッセージ）」 ---
    public void Init(int lane, int width, float targetTime, float speed) {
        this.lane = lane;
        this.targetTime = targetTime;
        this.scrollSpeed = speed;
        this.spawnTime = Time.time;

        // 1. 横位置(X)と幅(Scale)をセット
        // 12レーンある想定で、中心を0にする計算（調整してね！）
        float posX = (lane - 5.5f) * 1.0f; 
        transform.position = new Vector3(posX, 10f, 0f); // 最初は画面上の方へ
        transform.localScale = new Vector3(width, 0.2f, 1f); // 幅をプロセカ風に
    }

    void Update() {
        // 現在の曲の再生時間（AudioSourceから取ってもOK）
        // ここでは単純なTime.timeで「判定までの残り時間」を計算
        float currentTime = Time.time; 
        float timeRemaining = targetTime - currentTime;

        // 2. 位置の計算（判定ラインを Z=0 とした場合）
        // 残り時間にスピードをかけると、ちょうど 0秒で Z=0 に到達する
        float posZ = timeRemaining * scrollSpeed;
        
        transform.position = new Vector3(transform.position.x, transform.position.y, posZ);

        // 3. 判定ラインを通り過ぎて一定時間（1秒とか）経ったら消える
        if (timeRemaining < -1.0f) {
            Destroy(gameObject);
        }
    }
}
