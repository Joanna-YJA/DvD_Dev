package gov.dsta.dvd.dif.video;

public class WindowedEventRate {
	private static final long _1000L = 1000L;
	private double normalizedRate;
	private long windowSizeTicks;
	private long lastEventTicks;
	
	public WindowedEventRate(int aWindowSizeSeconds){
		windowSizeTicks = aWindowSizeSeconds * _1000L;
		lastEventTicks = System.currentTimeMillis();
	}
	
	public double newEvent(){
		long currentTicks = System.currentTimeMillis();
		long period = currentTicks - lastEventTicks;
		lastEventTicks = currentTicks;
		double normalizedFrequency = (double) windowSizeTicks / (double) period;
		double alpha = Math.min(1.0 / normalizedFrequency, 1.0);
		normalizedRate = (alpha * normalizedFrequency) + ((1.0- alpha) * normalizedRate);
		return getRate();
	}
	double maxCount = 100;
	double count = 0;
	long oldTime = System.currentTimeMillis();
	double fps = 0;
	public double newEvent2(){
		count++;
		if(count >= maxCount){
			count = 0;
			System.err.println(((System.currentTimeMillis() - oldTime)/1000));
			fps = maxCount/((System.currentTimeMillis() - oldTime)/1000.0); 
				oldTime =	System.currentTimeMillis();
			 
		}
		
		return fps;
	}
	
	public double getRate(){
		return normalizedRate * _1000L / windowSizeTicks;
	}
}
