using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.msgs.ev3_msgs
{
    public class Ev3PduSensor
    {
        protected internal readonly IPdu _pdu;

        public Ev3PduSensor(IPdu pdu)
        {
            _pdu = pdu;
        }

        private Ev3PduSensorHeader _head;
        public Ev3PduSensorHeader Head 
        {
            get
            {
                if (_head == null)
                {
                    _head = new Ev3PduSensorHeader(_pdu.GetData<IPdu>("head"));
                }
                return _head;
            }
        }

        private Ev3PduColorSensor[] _colorSensors;
        public Ev3PduColorSensor[] ColorSensors
        {
            get
            {
                if (_colorSensors == null)
                {
                    var sensorPdus = _pdu.GetDataArray<IPdu>("color_sensors");
                    _colorSensors = new Ev3PduColorSensor[sensorPdus.Length];
                    for (int i = 0; i < sensorPdus.Length; i++)
                    {
                        _colorSensors[i] = new Ev3PduColorSensor(sensorPdus[i]);
                    }
                }
                return _colorSensors;
            }
            set
            {
                _colorSensors = new Ev3PduColorSensor[value.Length];
                IPdu[] fieldPdus = new IPdu[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    fieldPdus[i] = value[i]._pdu;
                    _colorSensors[i] = value[i];
                }
                _pdu.SetData("color_sensors", fieldPdus);
            }
        }

        private Ev3PduTouchSensor[] _touchSensors;
        public Ev3PduTouchSensor[] TouchSensors
        {
            get
            {
                if (_touchSensors == null)
                {
                    var sensorPdus = _pdu.GetDataArray<IPdu>("touch_sensors");
                    _touchSensors = new Ev3PduTouchSensor[sensorPdus.Length];
                    for (int i = 0; i < sensorPdus.Length; i++)
                    {
                        _touchSensors[i] = new Ev3PduTouchSensor(sensorPdus[i]);
                    }
                }
                return _touchSensors;
            }
            set {
                _touchSensors = new Ev3PduTouchSensor[value.Length];
                IPdu[] fieldPdus = new IPdu[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    fieldPdus[i] = value[i]._pdu;
                    _touchSensors[i] = value[i];
                }
                _pdu.SetData("touch_sensors", fieldPdus);
            }
        }

        public uint[] MotorAngle
        {
            get => _pdu.GetDataArray<uint>("motor_angle");
            set => _pdu.SetData("motor_angle", value);
        }
    }
}