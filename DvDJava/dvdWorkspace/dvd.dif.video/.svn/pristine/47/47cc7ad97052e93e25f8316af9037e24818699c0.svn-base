package gov.dsta.dvd.dif.video.serial;

import java.io.ByteArrayInputStream;
import java.io.DataInputStream;
import java.io.InputStream;
import java.util.Scanner;

import com.fazecast.jSerialComm.SerialPort;
import com.fazecast.jSerialComm.SerialPortDataListener;
import com.fazecast.jSerialComm.SerialPortEvent;
import com.fazecast.jSerialComm.SerialPortIOException;
import com.fazecast.jSerialComm.SerialPortMessageListener;
import com.fazecast.jSerialComm.SerialPortPacketListener;

public class SerialPortReader{
	public static byte[] dataRece = new byte[14];
	public static boolean isStarted = false;
	public static int count = 0 ;
	public static void main(String[] args){
		
		System.out.println("\nUsing Library Version v" + SerialPort.getVersion());
		SerialPort[] ports = SerialPort.getCommPorts();
		System.out.println("\nAvailable Ports:\n");
		for (int i = 0; i < ports.length; ++i){
			System.out.println("   [" + i + "] " + ports[i].getSystemPortName() + ": " + ports[i].getDescriptivePortName() + " - " + ports[i].getPortDescription());
		}
		SerialPort ubxPort;
		System.out.print("\nChoose your desired serial port or enter -1 to specify a port directly: ");
		int serialPortChoice = 0;
		try {
			Scanner inputScanner = new Scanner(System.in);
			serialPortChoice = inputScanner.nextInt();
			inputScanner.close();
		} catch (Exception e) {}
		if (serialPortChoice == -1)
		{
			String serialPortDescriptor = "";
			System.out.print("\nSpecify your desired serial port descriptor: ");
			try {
				Scanner inputScanner = new Scanner(System.in);
				serialPortDescriptor = inputScanner.nextLine();
				inputScanner.close();
			} catch (Exception e) {}
			ubxPort = SerialPort.getCommPort(serialPortDescriptor);
		}
		else
			ubxPort = ports[serialPortChoice];

		System.out.println("\nPre-setting RTS: " + (ubxPort.setRTS() ? "Success" : "Failure"));
		boolean openedSuccessfully = ubxPort.openPort(0);
		System.out.println("\nOpening " + ubxPort.getSystemPortName() + ": " + ubxPort.getDescriptivePortName() + " - " + ubxPort.getPortDescription() + ": " + openedSuccessfully);
		if (!openedSuccessfully)
			return;
		ubxPort.setBaudRate(9600);
		
		// add the message listener
		MessageListener listener = new MessageListener();
		ubxPort.addDataListener(listener);
		try {
			Thread.sleep(5000);
		}
		catch (Exception e){
			e.printStackTrace();
		}
		
		
		/*
		ubxPort.setComPortTimeouts(SerialPort.TIMEOUT_READ_BLOCKING, 100, 0);
//		try
//		{
//			while(true)
//			{
//				byte[] newData = new byte[ubxPort.bytesAvailable()];
//
//				//System.out.println("Available: " + ubxPort.bytesAvailable());
//				int numRead = ubxPort.readBytes(newData, newData.length);
//				//System.out.println("Read " + numRead + " bytes.");
//				for(int i = 0 ; i < numRead ;  i++){
//					//		System.out.print(newData[i] + ", " );
//							
//							if(numRead - i > 2){
//								if(newData[i] == 125 && newData[i+1] == 125){
//									System.err.println("FOUND start");
//									dataRece = new byte[14];
//									dataRece[0] = 125;
//									dataRece[1] = 125;
//									isStarted = true;
//									count = 0;
//									continue;
//								}
//							}
//							if(isStarted && count <13){
//								count++;
//								dataRece[count] = newData[i];
//								//System.err.println("reading count " + count + " data : " + newData[i]);
//								continue;
//							}else{
//								System.err.println("FOUND END");
//								ByteArrayInputStream bytearrypacket = new ByteArrayInputStream(dataRece);
//								DataInputStream dataInputpacket = new DataInputStream(bytearrypacket);
//								try{
//									dataInputpacket.readByte();
//									dataInputpacket.readByte();
//									float roll = dataInputpacket.readFloat();
//									float pitch =  dataInputpacket.readFloat();
//									float yaw =  dataInputpacket.readFloat();
//									System.err.println("roll " + Math.toDegrees(roll)%360 + " pitch " + Math.toDegrees(pitch)%360 + " yaw " + Math.toDegrees(yaw)%360);
//								}catch(Exception e){
//									e.printStackTrace();
//								}
//								
//								isStarted = false;
//								count = 0;
//								
//							}
//							
//							
//						}
//				
//			}
//		} catch (Exception e) { e.printStackTrace(); }
		System.out.println("\nSwitching over to event-based reading");
		System.out.println("\nListening for any amount of data available\n");
		
		ubxPort.addDataListener(new SerialPortDataListener() {
			@Override
			public int getListeningEvents() { return SerialPort.LISTENING_EVENT_DATA_AVAILABLE; }
			@Override
			public void serialEvent(SerialPortEvent event)
			{
				SerialPort comPort = event.getSerialPort();
				byte[] newData = new byte[comPort.bytesAvailable()];
				int numRead = comPort.readBytes(newData, newData.length);
			//	System.out.println("Read " + numRead + " bytes.");
				for(int i = 0 ; i < numRead ;  i++){
			//		System.out.print(newData[i] + ", " );
					
					if(numRead - i > 2){
						if(newData[i] == 125 && newData[i+1] == 125){
							System.err.println("FOUND start");
							dataRece = new byte[14];
							dataRece[0] = 125;
							dataRece[1] = 125;
							isStarted = true;
							count = 0;
							continue;
						}
					}
					if(isStarted && count <13){
						count++;
						dataRece[count] = newData[i];
						System.err.println("reading count " + count + " data : " + newData[i]);
						if(count == 13){
							System.err.println("FOUND END");
							ByteArrayInputStream bytearrypacket = new ByteArrayInputStream(dataRece);
							DataInputStream dataInputpacket = new DataInputStream(bytearrypacket);
							try{
								byte id = dataInputpacket.readByte();
								byte id2 =  dataInputpacket.readByte();
								float roll = dataInputpacket.readFloat();
								float pitch =  dataInputpacket.readFloat();
								float yaw =  dataInputpacket.readFloat();
								System.err.println(id + " " + id2 + " roll " + Math.toDegrees(roll)%360 + " pitch " + Math.toDegrees(pitch)%360 + " yaw " + Math.toDegrees(yaw)%360);
							}catch(Exception e){
								e.printStackTrace();
							}
						}
						continue;
					}
					
					
				}
			}
		});
		*/
	}
	private static final class PacketListener implements SerialPortPacketListener
	{
		@Override
		public int getListeningEvents() { return SerialPort.LISTENING_EVENT_DATA_RECEIVED; }
		@Override
		public void serialEvent(SerialPortEvent event)
		{
			byte[] newData = event.getReceivedData();
			System.out.println("Received data of size: " + newData.length);
			for (int i = 0; i < newData.length; ++i)
				System.out.print((char)newData[i]);
			System.out.println("\n");
		}
		@Override
		public int getPacketSize() { return 100; }
	}
	
	private static final class MessageListener implements SerialPortMessageListener
	{
		public String byteToHex(byte num)
		{
			char[] hexDigits = new char[2];
			hexDigits[0] = Character.forDigit((num >> 4) & 0xF, 16);
			hexDigits[1] = Character.forDigit((num & 0xF), 16);
			return new String(hexDigits);
		}
		@Override
		public int getListeningEvents() { return SerialPort.LISTENING_EVENT_DATA_RECEIVED; }
		@Override
		public void serialEvent(SerialPortEvent event)
		{
			byte[] byteArray = event.getReceivedData();
			StringBuffer hexStringBuffer = new StringBuffer();
			for (int i = 0; i < byteArray.length; i++)
				hexStringBuffer.append(byteToHex(byteArray[i]));
			//System.out.println("Number of hex bytes: " + hexStringBuffer.length() * 0.5);
			//System.out.println("Received the following message: " + hexStringBuffer.toString());
			//System.out.println("Received the following message: " + byteArray);
			
			ByteArrayInputStream bytearrypacket = new ByteArrayInputStream(byteArray);
			DataInputStream dataInputpacket = new DataInputStream(bytearrypacket);
			try{
				// discard start headers
				dataInputpacket.readShort();
				
				// read msg id
				int id;
				id = Byte.toUnsignedInt(dataInputpacket.readByte());
				
				// attitude message
				if (id == 30)
				{
					float roll, pitch, yaw;
					
					roll = dataInputpacket.readFloat();
					pitch = dataInputpacket.readFloat();
					yaw = dataInputpacket.readFloat();
					
					//System.out.print("roll: " + roll);
					//System.out.print(" pitch: " + pitch);
					//System.out.println(" yaw: " + yaw);
				}
				
				// gps message
				if (id == 33) 
				{
					int lat, lon, alt, hdg;
					short vx, vy, vz;
					
					lat = dataInputpacket.readInt();
					lon = dataInputpacket.readInt();
					alt = dataInputpacket.readInt();
					
					vx =  dataInputpacket.readShort();
					vy =  dataInputpacket.readShort();
					vz =  dataInputpacket.readShort();
					
					hdg = Short.toUnsignedInt(dataInputpacket.readShort());
					
					
					System.out.print("lat: " + lat);
					System.out.print(" lon: " + lon);
					System.out.print(" alt: " + alt);
					System.out.print(" vx: " + vx);
					System.out.print(" vy: " + vy);
					System.out.print(" vz: " + vz);
					System.out.println(" hdg: " + hdg);
					
				}
				
				

			}catch(Exception e){
				e.printStackTrace();
			}
			
		}
		@Override
		public byte[] getMessageDelimiter() { return new byte[]{ (byte)0x7D, (byte)0x7D }; }
		@Override
		public boolean delimiterIndicatesEndOfMessage() { return false; }
	}
}
