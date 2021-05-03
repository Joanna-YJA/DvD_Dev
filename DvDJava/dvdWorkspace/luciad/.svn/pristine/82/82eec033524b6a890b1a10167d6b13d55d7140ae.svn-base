package gov.dsta.luciad;

import com.luciad.geodesy.TLcdGeodeticDatum;
import com.luciad.model.ILcdModelReference;
import com.luciad.reference.TLcdGeodeticReference;
import com.luciad.shape.ILcdPoint;
import com.luciad.view.lightspeed.controller.ruler.TLspRulerController;

public class DistanceMeasureController extends TLspRulerController{

	private ILcdModelReference reference;
	
	public DistanceMeasureController(){
		TLcdGeodeticDatum datum = new TLcdGeodeticDatum();
		reference = new TLcdGeodeticReference(datum);
	}
	
	public double measureDistance(ILcdPoint aPoint1, ILcdPoint aPoint2){
		return this.calculateDistance(aPoint1, aPoint2, reference, TLspRulerController.MeasureMode.MEASURE_GEODETIC);
	}
	
	public double measureDistance(ILcdPoint aPoint1, ILcdPoint aPoint2, MeasureMode mode){
		return this.calculateDistance(aPoint1, aPoint2, reference, mode);
	}
}
