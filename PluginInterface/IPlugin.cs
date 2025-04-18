using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;

namespace PluginInterface
{
    public interface IPlugin
    {
        string Name { get; }
        string Author { get; }
        Task TransformAsync(Bitmap bitmap, IProgress<int> progress, CancellationToken cancellationToken);
        void Transform(Bitmap bitmap);
    }

}
