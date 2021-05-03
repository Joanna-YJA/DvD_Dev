package gov.dsta.luciad.interfaces;

import java.awt.Rectangle;
import java.util.List;

public interface IViewControl {

	List<KImage> getAllObjectOnOffScreen();

	void centerOffScreenGIS(double x, double y, double z, double yaw, double pitch, double roll, double fov);

	void showHideOffScreenGIS(boolean isShow);

	void plotIconOnOffscreen(KImage graphic);

	void updateIconOnOffscreen(KImage modelKImage);

	void setOffScreenBound(Rectangle rect);

	void deleteIconOnOffscreen(KImage graphic);

	void init();

	double measureDistance(double x1, double y1, double z1, double x2, double y2, double z2);

	List<KImage> getAllObject();

	double checkAngleBetweenPoint(IKPoint startCoord, IKPoint endCoord);

	//for testing
	KImage getCurrentPosition();

}
