using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    interface IThumbnailCallbackReceiver
    {
        void ThumbRequestCallback(IAsyncResult result);
    }

    interface IImageCallbackReceiver
    {
        void ImageRequestCallback(IAsyncResult result);
    }
}
