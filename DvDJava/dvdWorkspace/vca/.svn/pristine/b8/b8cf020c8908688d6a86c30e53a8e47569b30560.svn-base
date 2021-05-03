package gov.dsta.vca.udp;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.SocketException;
import java.net.SocketTimeoutException;
import java.util.ArrayList;
import java.util.concurrent.LinkedBlockingQueue;


public class UdpServer {
	
	// UDP port number
	public static final int UDP_PORT_NUMBER = 4446;
	public static final int UDP_PACKET_SIZE = 64512;
	
	// timeout for UDP socket
//	public static final int UDP_RECEIVE_TIMEOUT = 1500;	// 1.5 seconds 	

	private LinkedBlockingQueue<Request> requestQueue = null;
	
	// when a message is received, the listeners' onReceived() will be called
	private ArrayList<IRequestListener> requestListener = null;
	

	private int udpPort = 0;
	
	private Thread udpServerThread = null;
	private Thread udpAgentThread = null;

	public UdpServer() {
		this(UDP_PORT_NUMBER, null);
	}
	public UdpServer(int udpPort) {
		this(udpPort, null);
	}

	public UdpServer(IRequestListener listener) {
		this(UDP_PORT_NUMBER, listener);
	}
	
	public UdpServer(int udpPort, IRequestListener listener) {
		this.udpPort = udpPort;
		
		requestQueue = new LinkedBlockingQueue<Request>();
		requestListener = new ArrayList<IRequestListener>();
		if (listener != null)
			requestListener.add(listener);
	}
	
	public void addRequestListener(IRequestListener listener){
		requestListener.add(listener);
	}
	
	
	public int getUdpPort() {
		return udpPort;
	}
	
	public void setUdpPort(int udpPort) {
		this.udpPort = udpPort;
	}
	
	private void startUdpReceiveAgent() {
		
		udpAgentThread = new Thread(()->{
			boolean isInterrupted = Thread.currentThread().isInterrupted();
			while(!isInterrupted) {
				try {
					Request request = requestQueue.take();
					//byte[] bytes = request.getData();
					try{
					for (IRequestListener listener : requestListener) {
						listener.onReceived(request);
					}
					}catch(Exception e){
//						e.printStackTrace();
					}
					


				} catch (InterruptedException e) {
					// TODO Auto-generated catch block
					Thread.currentThread().interrupt();
//					e.printStackTrace();
				}
				catch(IllegalMonitorStateException ex){
//					ex.printStackTrace();
				}
				isInterrupted = Thread.currentThread().isInterrupted();
			}
		});
		
		udpAgentThread.start();
	}
	
	// Session is established through a communication between C2 and Android via TCP: C2 send a command, and Android send image information
	public UdpServer start() {
		
		startUdpReceiveAgent();
		
		udpServerThread = new Thread(()->{
			
			DatagramSocket socket = null;
			
			// open a socket
			try {
				socket = new DatagramSocket(udpPort);
//				if (socket != null)
//					socket.setSoTimeout(UDP_RECEIVE_TIMEOUT);
			} catch(SocketException e) {
//				e.printStackTrace();
				socket = null;
			}
			
			if (socket != null) {
				boolean isInterrupted = Thread.currentThread().isInterrupted();
				while(!isInterrupted) {
					
					
					try {
//						System.out.println("RECEIVE NEW UDP MESSAGE");
						Request request = receiveMessage(socket);
						if (request != null){ //&& request.getCommand() != null){
							requestQueue.put(request);
						}
						
					} catch(SocketTimeoutException e) {
//						e.printStackTrace();
						
					} catch (IOException e) {
//						e.printStackTrace();
					} catch (InterruptedException e) {
						// TODO Auto-generated catch block
						Thread.currentThread().interrupt();
					}
					isInterrupted = Thread.currentThread().isInterrupted();
				}
			}
			
			if (socket != null)
				socket.close();
			
		});
		
		udpServerThread.start();
		
		return this;
	}
	
	// once finished frame sending, the API is invoked
	public void stop() {
		if (udpAgentThread != null)
			udpAgentThread.interrupt();
		
		if (udpServerThread != null)
			udpServerThread.interrupt();
		
		udpAgentThread = null;
		udpServerThread = null;
	}
	
	// receiving a frame from a UDP socket
	private static Request receiveMessage(DatagramSocket socket) throws IOException {
		
		// receive 1st packet
		
		int length = socket.getReceiveBufferSize();
		byte[] buf = new byte[length];
		DatagramPacket packet = new DatagramPacket(buf, buf.length);
		socket.receive(packet);		// blocks if no packet

		// full image size
		byte[] data = packet.getData();
		//System.out.println("message type id " + data[0] + " from " + packet.getAddress().getHostAddress());
		String address = packet.getAddress().getHostAddress();
		//MessagingService.entityConnectivityStatus.put(MessagingService.ipToEntity.get(address), STATUS.CONNECTED);

		
		Request request = new Request(data[0],data[1] ,data, address){
		};
		
		try {
			DetectionMessage msg = new DetectionMessage();
		//	System.err.println("new message");
			msg.readData(data);
			request.setCommand(msg);
		} catch (Exception e) {
			e.printStackTrace();
//			e.printStackTrace();
		}
		return request;
	}
	public void removeRequestListener(IRequestListener listener) {
		requestListener.remove(listener);
		
	}
	

}
