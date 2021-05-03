package gov.dsta.vca.udp;

import java.io.Serializable;
import java.util.zip.CRC32;

public abstract class Command implements Serializable {
	
	private String hostAddress = null;
	private int hostTcpPort = 0;
	
	public Command() {
		this(null, 0);
	}
	
	public Command(String hostAddress, int hostTcpPort) {
		this.hostAddress = hostAddress;
		this.hostTcpPort = hostTcpPort;
	}
	
	public String getHostAddress() {
		return hostAddress;
	}
	
	public void setHostAddress(String hostAddress) {
		this.hostAddress = hostAddress;
	}
	
	public int getHostTcpPort() {
		return hostTcpPort;
	}
	
	public void setHostTcpPort(int hostTcpPort) {
		this.hostTcpPort = hostTcpPort;
	}
	
	// subclass needs to implement
	public abstract byte[] getData();
	public abstract void readData(byte[] data);
	
	public  long getRefId(){
		return 0;
	}


	
	public abstract String getTopic();
	

	public long generateCRC32(byte[] data) {
		CRC32 crc32 = new CRC32();
		crc32.update(data);
		return crc32.getValue();
	}

}



