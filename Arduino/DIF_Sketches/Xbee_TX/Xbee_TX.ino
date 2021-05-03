/* For Arduino located on the UAV
 * The Arduino will take the telemetry values FROM the flight controller
 * and transmit wirelessly via the Xbee module
 */

// libraries
#include <SoftwareSerial.h>
#include <mavlink.h>  // change to path of whatever directory your libraries are located in. Make sure to include the mavlink.h header file from the ardupilot mega folder and not the common folder

// config
//#define SENDTOSERIAL
//#define DEBUG
//#define MSG_HEARTBEAT
//#define MSG_SYSSTATUS
#define MSG_ATTITUDE
#define MSG_GPOSINT
uint16_t pingInterval = 500;  // milliseconds

// Serial load from Flight controller
static const uint8_t FC_RX = 9;  
static const uint8_t FC_TX = 10;
SoftwareSerial SerialFC(FC_RX, FC_TX);

// Serial load to Xbee transmitter
static const uint8_t XB_RX = 11;
static const uint8_t XB_TX = 12;
SoftwareSerial SerialXB(XB_RX, XB_TX);

// Baud rate (we try to use a common baudRate across all serials)
static const uint32_t baudRate = 9600;

// global variables
mavlink_message_t msg;    
mavlink_status_t status;
uint8_t rawData;
unsigned long prev_Time;
bool flag_newMsg = false;

void setup () 
{
  SerialFC.begin(baudRate);
  SerialXB.begin(baudRate);
  Serial.begin(baudRate);
  Serial.println("--> Serial ports opened...");
  prev_Time = millis();
  
  #ifdef DEBUG
  Serial.begin (baudRate);
  Serial.println("--> Serial ports opened...");
  #endif
}

void loop () 
{
  // listen for telemetry from the flight controller
  SerialFC.listen();
  _MavLink_receive();

/*
  if (millis() - prev_Time > pingInterval)
  {
    _Xbee_send();
    
    prev_Time = millis();
  }
  */
}



void _MavLink_receive () 
{
  // reset msg
  msg.msgid = 255;  // 255 is used because it does not reference a valid msg

  // check for messages from FC
  if (SerialFC.available()) 
  {
    rawData = 0;
    rawData = SerialFC.read();
    if (mavlink_parse_char(MAVLINK_COMM_0, rawData, &msg, &status)) 
    {
      flag_newMsg = true;
      
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

      //test_GPS_msg();
    }
  }
}

void _Xbee_send() 
{
  if (!flag_newMsg) 
  {
    return;
  }
  
  // TODO: Send the message via Xbee protocol
  SerialXB.write ((byte*)&msg, sizeof(msg));

#ifdef SENDTOSERIAL
  Serial.write((byte*)&msg, sizeof(msg));
#endif

  flag_newMsg = false;
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
      
      Serial.println("--ATTITUDE--");
      Serial.println("|  Roll  |  Pitch  |  Yaw  |");
      Serial.print("|  ");Serial.print(attitude.roll);
      Serial.print("  |  ");Serial.print(attitude.pitch);
      Serial.print("  |  ");Serial.print(attitude.yaw);Serial.println(" |");
      Serial.println();
    #endif

      mavlink_attitude_t attitude;
      mavlink_msg_attitude_decode(&msg, &attitude);
      
      byte data[15];
      // message start delimiter
      data[0] = 125;
      data[1] = 125;

      // message id
      data[2] = id;

      //  we flip the bytes to convert to Big Endian in preperation to receive on Java side
      byte *b = (byte *)&attitude.roll;
      data[3] = b[3];
      data[4] = b[2];
      data[5] = b[1];
      data[6] = b[0];
      
      byte *c = (byte *)&attitude.pitch;
      data[7] = c[3];
      data[8] = c[2];
      data[9] = c[1];
      data[10] = c[0];

      byte *d = (byte *)&attitude.yaw;
      data[11] = d[3];
      data[12] = d[2];
      data[13] = d[1];
      data[14] = d[0];

      SerialXB.write((byte*)&data, sizeof(data));
      Serial.println(attitude.roll);
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

    mavlink_global_position_int_t pos;
    mavlink_msg_global_position_int_decode(&msg, &pos);

    byte data[23];
    // message start delimiter
    data[0] = 125;
    data[1] = 125;

    // message id
    data[2] = id;

    //  we flip the bytes to convert to Big Endian in preperation to receive on Java side
    byte *b = (byte *)&pos.lat;
    data[3] = b[3];
    data[4] = b[2];
    data[5] = b[1];
    data[6] = b[0];
    
    byte *c = (byte *)&pos.lon;
    data[7] = c[3];
    data[8] = c[2];
    data[9] = c[1];
    data[10] = c[0];

    byte *d = (byte *)&pos.alt;
    data[11] = d[3];
    data[12] = d[2];
    data[13] = d[1];
    data[14] = d[0];

    byte *e = (byte *)&pos.vx;
    data[15] = e[1];
    data[16] = e[0];

    byte *f = (byte *)&pos.vy;
    data[17] = f[1];
    data[18] = f[0];

    byte *g = (byte *)&pos.vz;
    data[19] = g[1];
    data[20] = g[0];

    byte *h = (byte *)&pos.hdg;
    data[21] = h[1];
    data[22] = h[0];
    
    SerialXB.write((byte*)&data, sizeof(data));
    Serial.println("Lat: "+ pos.lat);
}

void test_GPS_msg () 
{
    // msg id
    uint8_t id = 33;

    int32_t lat, lon, alt;
    int16_t vx, vy, vz;
    uint16_t hdg;

    lat = -111111;
    lon = 122222;
    alt = 133333;

    vx = 11111;
    vy = 12222;
    vz = 13333;
    hdg = 240;

    byte data[23];
    // message start delimiter
    data[0] = 125;
    data[1] = 125;

    // message id
    data[2] = id;

    //  we flip the bytes to convert to Big Endian in preperation to receive on Java side
    byte *b = (byte *)&lat;
    data[3] = b[3];
    data[4] = b[2];
    data[5] = b[1];
    data[6] = b[0];
    
    byte *c = (byte *)&lon;
    data[7] = c[3];
    data[8] = c[2];
    data[9] = c[1];
    data[10] = c[0];

    byte *d = (byte *)&alt;
    data[11] = d[3];
    data[12] = d[2];
    data[13] = d[1];
    data[14] = d[0];

    byte *e = (byte *)&vx;
    data[15] = e[1];
    data[16] = e[0];

    byte *f = (byte *)&vy;
    data[17] = f[1];
    data[18] = f[0];

    byte *g = (byte *)&vz;
    data[19] = g[1];
    data[20] = g[0];

    byte *h = (byte *)&hdg;
    data[21] = h[1];
    data[22] = h[0];
    
    SerialXB.write((byte*)&data, sizeof(data));
    Serial.println(lat);
}
