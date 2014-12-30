using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandererPanoramasEditor
{
    interface IServerCallback
    {
        void WaitingRoomListRequestCallback(IAsyncResult result);
        void ThumbnailRequestCallback(IAsyncResult result);
        void ImageRequestCallback(IAsyncResult result);
    }
}
