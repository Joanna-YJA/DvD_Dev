/* For Arduino located on the PC
 * The Arduino will receive telemetry values FROM the Xbee module
 */

// libraries
#include <SoftwareSerial.h>
#include <mavlink.h>  // change to path of whatever directory your libraries are located in. Make sure to include the mavlink.h header file from the ardupilot mega folder and not the common folder

// config
#define DEBUG
//#define MSG_HEARTBEAT
//#define MSG_SYSSTATUS
#define MSG_ATTITUDE
//#define MSG_GPOSINT

// Serial load from Xbee receiver
static const uint8_t XB_RX = 11;
static const uint8_t XB_TX = 12;
SoftwareSerial SerialXB(XB_RX, XB_TX);

// Baud rate (we try to use a common baudRate across all serials)
static const uint32_t baudRate = 57600;

// global variables
mavlink_message_t msg;    
mavlink_status_t status;
uint8_t rawData;
bool flagToSend = false;   // sets a flag to transmit the current message

void setup() 
{
  SerialXB.begin(baudRate);

  #ifdef DEBUG
  Serial.begin(baudRate);
  Serial.println("--> Serial ports opened...listening from Xbee...");
  #endif
}

void loop() 
{
  // listen for telemetry from the Xbee module
  SerialXB.listen();
  _XBee_receive();
}




void _XBee_receive () 
{
   // check for messages from Xbee
   if (SerialXB.available()) 
   {
      rawData = 0;
      rawData = SerialXB.read();
      if (mavlink_parse_char(MAVLINK_COMM_0, rawData, &msg, &status)) 
      {
#ifdef DEBUG
  Serial.write((byte*)&msg, sizeof(msg));
  
#endif
        
        #ifdef MSG_HEARTBEAT
          _flag_HEARTBEAT();
        #endif
  
        #ifdef MSG_SYSSTATUS
          _flag_SYSSTATUS();
        #endif
  
        #ifdef MSG_ATTITUDE
          _flag_ATTITUDE();
        #endif
  
        #ifdef MSG_GPOSINT
          _flag_GPOSINT();
        #endif
      }
   }
}


void _flag_HEARTBEAT () 
{
    // msg id
    uint8_t id = 0;
    
    if (msg.msgid != id) 
    {
      return;
    }
    
    #ifdef DEBUG
      mavlink_heartbeat_t hb;
      mavlink_msg_heartbeat_decode(&msg,&hb);

      Serial.println("--HEARTBEAT--");
      Serial.print(millis());
      Serial.print("\ncustom_mode: ");Serial.println(hb.custom_mode);
      Serial.print("Type: ");Serial.println(hb.type);
      Serial.print("autopilot: ");Serial.println(hb.autopilot);
      Serial.print("base_mode: ");Serial.println(hb.base_mode);
      Serial.print("system_status: ");Serial.println(hb.system_status);
      Serial.print("mavlink_version: ");Serial.println(hb.mavlink_version);
      Serial.println();
    #endif

    // flag the message for transmission
    flagToSend = true;
}

void _flag_SYSSTATUS () 
{
    // msg id
    uint8_t id = 1;
    
    if (msg.msgid != id) 
    {
      return;
    }
    
    #ifdef DEBUG
      mavlink_sys_status_t sys_status;
      mavlink_msg_sys_status_decode(&msg, &sys_status);
      
      Serial.println("--SYSTEM STATUS--");
      Serial.print("[Bat (V): ");
      Serial.print(sys_status.voltage_battery);
      Serial.print("], [Bat (A): ");
      Serial.print(sys_status.current_battery);
      Serial.print("], [Comms loss (%): ");
      Serial.print(sys_status.drop_rate_comm);
      Serial.println("]");
      Serial.println();
    #endif

    // flag the message for transmission
    flagToSend = true;
}

void _flag_ATTITUDE () 
{
    // msg id
    uint8_t id = 30;
    
    if (msg.msgid != id) 
    {
      return;
    }
    
    #ifdef DEBUG
    /*
     *uint32_t time_boot_ms; ///< Timestamp (milliseconds since system boot)
     *float roll; ///< Roll angle (rad)
     *float pitch; ///< Pitch angle (rad)
     *float yaw; ///< Yaw angle (rad)
     *float rollspeed; ///< Roll angular speed (rad/s)
     *float pitchspeed; ///< Pitch angular speed (rad/s)
     *float yawspeed; ///< Yaw angular speed (rad/s)
    */
    
      mavlink_attitude_t attitude;
      mavlink_msg_attitude_decode(&msg, &attitude);

/*
      byte data[14];
      data[0] = 125;
      data[1] = 125;
      
      byte *b = (byte *)&attitude.roll;
      data[2] = b[0];
      data[3] = b[1];
      data[4] = b[2];
      data[5] = b[3];
      
      *b = (byte *)&attitude.pitch;
      data[6] = b[0];
      data[7] = b[1];
      data[8] = b[2];
      data[9] = b[3];

       *b = (byte *)&attitude.yaw;
      data[10] = b[0];
      data[11] = b[1];
      data[12] = b[2];
      data[13] = b[3];

      //Serial.println((byte)data[2], sizeof(4));
      Serial.write((byte*)&data, sizeof(data));
      */
      
      /*
      Serial.println("--ATTITUDE--");
      Serial.println("|  Roll  |  Pitch  |  Yaw  |");
      Serial.print("|  ");Serial.print(attitude.roll);
      Serial.print("  |  ");Serial.print(attitude.pitch);
      Serial.print("  |  ");Serial.print(attitude.yaw);Serial.println(" |");
      Serial.println();

     */
    #endif

    // flag the message for transmission
    flagToSend = true;
}

void _flag_GPOSINT () 
{
    // msg id
    uint8_t id = 33;
    
    if (msg.msgid != id) 
    {
      return;
    }
    
    #ifdef DEBUG
     /*
      *uint32_t time_boot_ms; ///< Timestamp (milliseconds since system boot)
      *int32_t lat; ///< Latitude, expressed as * 1E7
      *int32_t lon; ///< Longitude, expressed as * 1E7
      *int32_t alt; ///< Altitude in meters, expressed as * 1000 (millimeters), above MSL
      *int32_t relative_alt; ///< Altitude above ground in meters, expressed as * 1000 (millimeters)
      *int16_t vx; ///< Ground X Speed (Latitude), expressed as m/s * 100
      *int16_t vy; ///< Ground Y Speed (Longitude), expressed as m/s * 100
      *int16_t vz; ///< Ground Z Speed (Altitude), expressed as m/s * 100
      *uint16_t hdg; ///< Compass heading in degrees * 100, 0.0..359.99 degrees. If unknown, set to: 65535
     */
         
      mavlink_global_position_int_t pos;
      mavlink_msg_global_position_int_decode(&msg, &pos);
      
      Serial.println("--GLOBAL POSITION--");
      Serial.print("Latitude: ");
      Serial.print(pos.lat);
      Serial.print(" Longitude: ");
      Serial.print(pos.lon);
      Serial.print(" Altitude: ");
      Serial.print(pos.alt);
      Serial.println();
    #endif

    // flag the message for transmission
    flagToSend = true;
}

float bytesToFloat(uchar b0, uchar b1, uchar b2, uchar b3) 
{ 
    uchar byte_array[] = { b3, b2, b1, b0 };
    float result;
    std::copy(reinterpret_cast<const char*>(&byte_array[0]),
              reinterpret_cast<const char*>(&byte_array[4]),
              reinterpret_cast<char*>(&result));
    return result;
} 
