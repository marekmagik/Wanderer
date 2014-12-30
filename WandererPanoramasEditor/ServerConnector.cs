using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WandererPanoramasEditor
{
    class ServerConnector
    {
        #region Members
        private const String _applicationName="/Wanderer";
        private const String _baseRequest = "http://{0}:{1}"+_applicationName+"{2}";
        #endregion

        #region Methods
        public Boolean SendDataToServer(byte [] image, ImageMetadata metadata)
        {
            Boolean result = false;

            String requestPart = "";
            if(ConfigurationFactory.GetConfiguration().Mode.Equals(Modes.AdminMode))
                requestPart = "/api/photos/insert/admin/";
            else if (ConfigurationFactory.GetConfiguration().Mode.Equals(Modes.NormalMode))
                requestPart = "/api/photos/insert/normal/";

            String json = JsonConvert.SerializeObject(metadata);
            byte[] metadataBytes = System.Text.Encoding.UTF8.GetBytes(json);

            Configuration configuration = ConfigurationFactory.GetConfiguration();
            String url = String.Format(_baseRequest, configuration.Address, configuration.Port, requestPart + metadataBytes.Length+"/"+image.Length);

            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            request.ContentLength = image.Length+metadataBytes.Length;
            request.ContentType = "application/octet-stream";
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(metadataBytes, 0, metadataBytes.Length);
            dataStream.Write(image, 0, image.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            if ("OK".Equals(((HttpWebResponse)response).StatusDescription))
                result = true;
            response.Close();

            return result;
        }

        public void GetWaitingRoomContent(IServerCallback callback)
        {
            String requestPart = "/api/places/get/all";

            Configuration configuration = ConfigurationFactory.GetConfiguration();
            String url = String.Format(_baseRequest, configuration.Address, configuration.Port, requestPart);

            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.WaitingRoomListRequestCallback),request);

        }

        public void GetThumbnail(String hash, IServerCallback callback)
        {
            String requestPart = "/api/photos/get/waiting/thumbnail/";

            Configuration configuration = ConfigurationFactory.GetConfiguration();
            String url = String.Format(_baseRequest, configuration.Address, configuration.Port, requestPart+hash);

            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.ThumbnailRequestCallback), request);
        }

        public void GetImage(String hash, IServerCallback callback)
        {
            String requestPart = "/api/photos/get/waiting/";

            Configuration configuration = ConfigurationFactory.GetConfiguration();
            String url = String.Format(_baseRequest, configuration.Address, configuration.Port, requestPart + hash);

            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.ImageRequestCallback), request);
        }

        public Boolean DeletePlaceFromWaitingRoom(String hash)
        {
            String requestPart = "/api/places/delete/waiting/";
            Boolean result = false;

            Configuration configuration = ConfigurationFactory.GetConfiguration();
            String url = String.Format(_baseRequest, configuration.Address, configuration.Port, requestPart + hash);

            WebRequest request = WebRequest.Create(url);
            request.Method = "DELETE";
            WebResponse response = request.GetResponse();

            if ("OK".Equals(((HttpWebResponse)response).StatusDescription))
                result = true;
            response.Close();

            return result;
        }

        #endregion
    }
}
