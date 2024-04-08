using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalImageLoader : MonoBehaviour
{
    public TextAsset _imgAssets;
    public bool needDecompress;
    // Start is called before the first frame update
    private RawImage _img;
    void Start()
    {
        _img = GetComponent<RawImage>();
        _img.texture = new Texture2D(2,2, TextureFormat.ARGB32, false);
        LoadImage();
    }

    async void LoadImage()
    {
        byte[] bytes = _imgAssets.bytes;
        ((Texture2D)(_img.texture)).LoadImage(bytes);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
