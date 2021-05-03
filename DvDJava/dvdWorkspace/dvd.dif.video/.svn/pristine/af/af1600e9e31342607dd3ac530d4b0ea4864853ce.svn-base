package gov.dsta.dvd.dif.video.model;

import java.io.DataInputStream;

import gov.dsta.dvd.dif.video.Main;
import gov.dsta.dvd.dif.video.WindowedEventRate;
import gov.dsta.dvd.dif.video.udp.Command;

public class TelemeteryMessage extends Command{
	private int id = 0;
	private double longitude;
	private double latitude;
	private double altitude;
	private float yaw = 0;
	private float pitch = 0;
	private float roll = 0;
	
	
	public int getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public double getLongitude() {
		return longitude;
	}

	public void setLongitude(double longitude) {
		this.longitude = longitude;
	}

	public double getLatitude() {
		return latitude;
	}

	public void setLatitude(double latitude) {
		this.latitude = latitude;
	}

	public double getAltitude() {
		return altitude;
	}

	public void setAltitude(double altitude) {
		this.altitude = altitude;
	}

	public float getYaw() {
		return yaw;
	}
	
	public float getYaw_DEG() {
		return (float) (yaw * (180.0/Math.PI));
	}

	public void setYaw(float yaw) {
		this.yaw = yaw;
	}

	public float getPitch() {
		return pitch;
	}
	
	public float getPitch_DEG() {
		return (float) (pitch * (180.0/Math.PI));
	}

	public void setPitch(float pitch) {
		this.pitch = pitch;
	}

	public float getRoll() {
		return roll;
	}
	
	public float getRoll_DEG() {
		return (float) (roll * (180.0/Math.PI));
	}

	public void setRoll(float roll) {
		this.roll = roll;
	}

	@Override
	public String toString() {
		return "TelemeteryMessage [id=" + id + ", longitude=" + longitude + ", latitude=" + latitude + ", altitude="
				+ altitude + ", yaw=" + yaw + ", pitch=" + pitch + ", roll=" + roll + "]";
	}
	
	

	@Override
	public byte[] getData() {
		// TODO Auto-generated method stub
		return null;
	}
	WindowedEventRate attitudeRate = new WindowedEventRate(1);
	WindowedEventRate gpsRate = new WindowedEventRate(1);

	@Override
	public void read_Attitude(DataInputStream dataInputpacket) {
		//ByteArrayInputStream bytearrypacket = new ByteArrayInputStream(data);
		//DataInputStream dataInputpacket = new DataInputStream(bytearrypacket);
		try{
			
			//id = Byte.toUnsignedInt(dataInputpacket.readByte());
			
			roll = -dataInputpacket.readFloat();		// roll is inverted for some reason
			pitch = dataInputpacket.readFloat();
			yaw = dataInputpacket.readFloat();
			Main.logger.info("Read Attitude| " + " Yaw: " + roll + " pitch: " + pitch + " roll: " + yaw + " hz: " + attitudeRate.newEvent() );
			
//        	System.out.print("roll: " + this.getRoll_DEG());
//			System.out.print(" pitch: " + this.getPitch_DEG());
//			System.out.println(" yaw: " + this.getYaw_DEG());

		}catch(Exception e){
			e.printStackTrace();
		}
	}
	
	@Override
	public void read_GPS (DataInputStream dataInputpacket) {
		//ByteArrayInputStream bytearrypacket = new ByteArrayInputStream(data);
		//DataInputStream dataInputpacket = new DataInputStream(bytearrypacket);
		try{
			
			//id = Byte.toUnsignedInt(dataInputpacket.readByte());
			
			int lat, lon, alt;
			
			lat = dataInputpacket.readInt();
			lon = dataInputpacket.readInt();
			alt = dataInputpacket.readInt();
			
			latitude = lat / 10000000.0;
			longitude = lon / 10000000.0;
			altitude = alt / 1000.0;
			
			//vx =  dataInputpacket.readShort();
			//vy =  dataInputpacket.readShort();
			//vz =  dataInputpacket.readShort();
			
			//hdg = Short.toUnsignedInt(dataInputpacket.readShort());
			
//			System.out.print("lat: " + latitude);
//			System.out.print(" lon: " + longitude);
//			System.out.println(" alt: " + altitude);
			//System.out.print(" vx: " + vx);
			//System.out.print(" vy: " + vy);
			//System.out.println(" vz: " + vz);
			//System.out.println(" hdg: " + hdg);
			Main.logger.info("Read GPS| "+"Lon: " + longitude + " Lat: " + latitude + " Alt: " + altitude + " hz: " + gpsRate.newEvent() );

		}catch(Exception e){
			e.printStackTrace();
		}
	}
	
	/*
	public void readData(byte[] data) {
		ByteArrayInputStream bytearrypacket = new ByteArrayInputStream(data);
		DataInputStream dataInputpacket = new DataInputStream(bytearrypacket);
		try{
			
			id = dataInputpacket.readInt();
			longitude = dataInputpacket.readDouble();
			latitude = dataInputpacket.readDouble();
			altitude = dataInputpacket.readDouble();
			yaw = dataInputpacket.readDouble();
			pitch = dataInputpacket.readDouble();
			roll = dataInputpacket.readDouble();
			

		}catch(Exception e){
			e.printStackTrace();
		}
		
	}
	*/

	@Override
	public String getTopic() {
		// TODO Auto-generated method stub
		return null;
	}

}
