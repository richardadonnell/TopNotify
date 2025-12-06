using IgniteView.Core;
using MimeMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using WatsonWebserver.Core;

namespace TopNotify.GUI
{
    internal static class WallpaperFinder
    {
        // Hardcoded path is intentional: This is a workaround for UWP file system virtualization.
        // The UWP runtime won't let us read from AppData, so we copy the wallpaper to Public Downloads.
#pragma warning disable S1075 // URIs should not be hardcoded
        private const string CopiedWallpaperPath = @"C:\Users\Public\Downloads\topnotify_tempwallpaper.jpg";
#pragma warning restore S1075

        public static async Task WallpaperRoute(HttpContextBase ctx)
        {
            if (
                ctx.Request.Url != null &&
                CopyWallpaper() != null
            )
            {

                // Send The Current Wallpaper
                var wallpaperFile = CopyWallpaper()!;

                var fileStream = new FileStream(wallpaperFile, FileMode.Open, FileAccess.Read);

                if (fileStream.CanSeek)
                {
                    ctx.Response.ContentLength = fileStream.Length;
                }

                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "image/jpeg";
                await ctx.Response.Send(fileStream.Length, fileStream);

                await fileStream.DisposeAsync();
            }

            ctx.Response.StatusCode = 404;
        }

        public static string? CopyWallpaper()
        {
            //Workaround For File System Write Virtualization
            //The UWP Runtime Won't Let Us Read From AppData
            //So Call CMD To Copy It Into A Location That We Can Access

            if (File.Exists(CopiedWallpaperPath))
            {
                return CopiedWallpaperPath;
            }

            Util.SimpleCMD($"copy /b/v/y \"%APPDATA%\\Microsoft\\Windows\\Themes\\TranscodedWallpaper\" \"{CopiedWallpaperPath}\"");

            return File.Exists(CopiedWallpaperPath) ? CopiedWallpaperPath : null;
        }

        public static void CleanUp()
        {
            if (File.Exists(CopiedWallpaperPath))
            {
                File.Delete(CopiedWallpaperPath);
            }
        }
    }
}
