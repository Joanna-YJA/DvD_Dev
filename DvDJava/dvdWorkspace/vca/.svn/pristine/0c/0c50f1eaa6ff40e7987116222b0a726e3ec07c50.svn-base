package gov.dsta.vca.udp;

public  class Request {

	private byte requestType = 0;
	private byte requestSubType = 0;
	private byte[] data;
	private String ipAddress = null;
	private Command command;
	public Request() {
		
	}
		
	public Request(byte requestType, byte[] data, String ipAddress) {
		this.requestType = requestType;
		this.data = data;
		this.ipAddress = ipAddress;
	}

	
	public Request(byte requestType,byte requestSubType, byte[] data, String ipAddress) {
		this.requestType = requestType;
		this.requestSubType = requestSubType;
		this.data = data;
		this.ipAddress = ipAddress;
	}
	public void setCommand(Command command){
		this.command = command;
	}
	public byte getRequestType()
	{
		return requestType;
	}
	public byte getRequestSubType()
	{
		return requestSubType;
	}
	
	public byte[] getData(){
		return data;
	}
	public Command getCommand(){
		return command;
	}

	public String getIpAddress() {
		return ipAddress;
	}

	public void setIpAddress(String ipAddress) {
		this.ipAddress = ipAddress;
	}

}
