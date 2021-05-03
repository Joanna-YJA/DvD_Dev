package gov.dsta.dvd.dif.video.model;

public class LatLonHeightPointModel {
	double lon;
	double lat;
	double height;
	float yaw;
	float pitch;
	float roll;

	public float getYaw() {
		return yaw;
	}
	public void setYaw(float yaw) {
		this.yaw = yaw;
	}
	public float getPitch() {
		return pitch;
	}
	public void setPitch(float pitch) {
		this.pitch = pitch;
	}
	public float getRoll() {
		return roll;
	}
	public void setRoll(float roll) {
		this.roll = roll;
	}
	public double getLon() {
		return lon;
	}
	public void setLon(double lon) {
		this.lon = lon;
	}
	public double getLat() {
		return lat;
	}
	public void setLat(double lat) {
		this.lat = lat;
	}
	public double getHeight() {
		return height;
	}
	public void setHeight(double height) {
		this.height = height;
	}
	public LatLonHeightPointModel(double lon, double lat, double height) {
		super();
		this.lon = lon;
		this.lat = lat;
		this.height = height;
	}
	
}
