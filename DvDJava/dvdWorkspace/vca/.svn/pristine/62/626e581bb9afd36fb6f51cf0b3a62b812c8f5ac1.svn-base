package gov.dsta.vca.util;

import java.awt.image.BufferedImage;
import java.awt.image.DataBufferByte;
import java.nio.ByteBuffer;

import org.opencv.core.CvType;
import org.opencv.core.Mat;
import org.opencv.imgproc.Imgproc;

import javafx.application.Platform;
import javafx.beans.property.ObjectProperty;
import javafx.embed.swing.SwingFXUtils;
import javafx.scene.image.Image;
import javafx.scene.image.PixelReader;
import javafx.scene.image.WritablePixelFormat;


public final class Utils
{
	
	private static final String MSG_PREFIX = "@VCA|UTILS : ";
	
	public static Mat image2Mat(Image frame)
	{	
		int width = Double.valueOf(frame.getWidth()).intValue();
		int height = Double.valueOf(frame.getHeight()).intValue();
		//System.err.println(width + " " +height);
		byte[] buffer = new byte[width * height *4];
		
		PixelReader reader = frame.getPixelReader();
		WritablePixelFormat<ByteBuffer> format = WritablePixelFormat.getByteBgraInstance();
		reader.getPixels(0, 0, width, height, format, buffer, 0,  width*4);
		Mat mat = new Mat(height, width, CvType.CV_8UC4);
		
		mat.put(0, 0, buffer);
		Mat result = new Mat();
		Imgproc.cvtColor(mat, result, Imgproc.COLOR_BGRA2BGR);

		
		return result;
	}	

	public static Image mat2Image(Mat frame)
	{
		try
		{
			return SwingFXUtils.toFXImage(matToBufferedImage(frame), null);
		}
		catch (Exception e)
		{
			return null;
		}
	}

	public static <T> void onFXThread(final ObjectProperty<T> property, final T value)
	{
		Platform.runLater(() -> {
			property.set(value);
		});
	}
	
	private static BufferedImage matToBufferedImage(Mat original)
	{
		// init
		BufferedImage image = null;
		int width = original.width(), height = original.height(), channels = original.channels();
		byte[] sourcePixels = new byte[width * height * channels];
		original.get(0, 0, sourcePixels);
		
		if (original.channels() > 1)
		{
			image = new BufferedImage(width, height, BufferedImage.TYPE_3BYTE_BGR);
		}
		else
		{
			image = new BufferedImage(width, height, BufferedImage.TYPE_BYTE_GRAY);
		}
//		final byte[] targetPixels = ((DataBufferByte) image.getRaster().getDataBuffer()).getData();
		System.arraycopy(sourcePixels, 0, ((DataBufferByte) image.getRaster().getDataBuffer()).getData(), 0, sourcePixels.length);
		
		
		return image;
	}
}
