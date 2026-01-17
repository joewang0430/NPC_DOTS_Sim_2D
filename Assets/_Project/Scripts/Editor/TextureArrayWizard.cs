using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureArrayWizard : ScriptableWizard
{
    [Tooltip("拖入所有的帧图片，顺序就是动画播放顺序")]
    public Texture2D[] textures;

    [MenuItem("Tools/Create Texture Array")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<TextureArrayWizard>(
            "Create Texture Array", "Create");
    }

    void OnWizardCreate()
    {
        if (textures.Length == 0) return;

        // 获取第一张图的属性作为基准
        Texture2D t0 = textures[0];
        // 创建 Texture2DArray
        // 参数：宽, 高, 切片数量(图的数量), 格式, 是否有Mipmap (改为 false 以解决拉远变白问题)
        Texture2DArray textureArray = new Texture2DArray(
            t0.width, t0.height, textures.Length, t0.format, false); // false = 关闭 mipmap

        // 应用过滤模式（强制 Point 以获得清晰的像素风）
        textureArray.filterMode = FilterMode.Point;
        textureArray.wrapMode = TextureWrapMode.Clamp;

        // 逐张填充
        for (int i = 0; i < textures.Length; i++)
        {
            // 复制像素数据（确保每张图也是可读的，或者由 Unity 自动处理）
            // 注意：这要求原图格式完全一致
            Graphics.CopyTexture(textures[i], 0, 0, textureArray, i, 0);
        }

        // 保存文件
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Texture Array", 
            "NewTextureArray", 
            "asset", 
            "Please enter a file name to save the texture array to");

        if (path.Length != 0)
        {
            AssetDatabase.CreateAsset(textureArray, path);
            Debug.Log("Saved Texture2DArray to " + path);
        }
    }
}