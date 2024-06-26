using Android.Opengl;

namespace ColorMC.Android.GLRender;

public static class GLHelper
{
    public static int CreateTexture()
    {
        int[] textures = new int[1];
        GLES20.GlGenTextures(1, textures, 0);
        GLES20.GlBindTexture(GLES20.GlTexture2d, textures[0]);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS, GLES20.GlClampToEdge);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapT, GLES20.GlClampToEdge);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlLinear);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlLinear);
        return textures[0];
    }

    public static void DeleteTexture(int texId)
    {
        int[] textures = [texId];
        GLES20.GlDeleteTextures(1, textures, 0);
    }

    public static string GetFileName(this GameRender.RenderType type)
    {
        return type switch
        {
            GameRender.RenderType.zink => "libOSMesa_8.so",
            _ => "libgl4es_114.so"
        };
    }

    public static string GetName(this GameRender.RenderType type)
    {
        return type switch
        {
            GameRender.RenderType.zink => "zink",
            _ => "gl4es"
        };
    }
}
