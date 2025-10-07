using UnityEngine;
using UnityEngine.Rendering;

public class SpectrumLiner : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private AudioSource audiosource;

    [SerializeField] private int visible = 128; // 描画数
    [SerializeField] private float waveLenght;  // 波形の広さ
    [SerializeField] private float yLength;     // 波形の高さ

    private const int FFT_RESOLUTION = 128; // FFTのサンプル数
    private float[] spectrum = null;        // スペクトラム配列
    private Vector3[] points = null;        // 描画ポイント配列


    void Start()
    {
        // 配列用メソッド
        Prepare();
    }
    /// <summary>
    /// スペクトラムと描画ポイントの配列を作るメソッド
    /// </summary>
    public void Prepare()
    {
        // スペクトラム配列(FFTのサンプル数)を作る
        spectrum = new float[FFT_RESOLUTION];
        // 描画ポイント配列(描画したい数)を作る
        points = new Vector3[visible + 1];
    }

    // Update is called once per frame
    void Update()
    {
        // 描画用メソッド
        LineRender();
    }


    /// <summary>
    /// スペクトラムの配列に沿ってラインを描画するメソッド
    /// </summary>
    private void LineRender()
    {
        // スペクトラムの数値データをスペクトラム配列に入れる
        audiosource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        //// 波形の広さから「描画のスタート地点」と「データの感覚」を求める
        var xStart = -waveLenght / 2;
        var xStep = waveLenght / spectrum.Length;

        var r = spectrum[0] * yLength;
        
        // var rad = Mathf.Deg2Rad;

        for (var i = 0; i < visible; i++)
        {
			// スペクトラムの「データ数値」からデカルト座標(x,y)を求める

			var y = spectrum[i] * yLength;
            var x = xStart + xStep * i;

            // 描画するポイントの配列に「求めたデカルト座標の位置」を入れる
            points[i] = new Vector3(x, y, 0) + transform.position;

            if (points == null) return; 

            // ラインレンダラーのポイント数を[描画ポイント配列の数]に合わせて
            // 各ポイントに「ポイントの配列の位置（求めた位置情報）」を設定する
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);


        }
    }
}
