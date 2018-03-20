using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class SceneModel
    {
        private QR_SCENE_Type _type;
        public SceneModel( QR_SCENE_Type type )
        {
            _type = type;
        }
        public SceneModel( QR_SCENE_Type type , object obj )
        {
            _type = type;
            Object = obj;
        }
        public QR_SCENE_Type SceneType
        {
            get { return _type; }
            set { _type = value; }
        }

        public object Object { get; set; }
    }
}
