package gov.dsta.luciad;

import org.hiranabe.vecmath.Vector3d;

import com.luciad.geodesy.TLcdGeodeticDatum;
import com.luciad.reference.ILcdGeoReference;
import com.luciad.reference.TLcdGeocentricReference;
import com.luciad.reference.TLcdGeodeticReference;
import com.luciad.reference.TLcdTopocentricReference;
import com.luciad.shape.ILcdPoint;
import com.luciad.shape.shape3D.TLcdLonLatHeightPoint;
import com.luciad.shape.shape3D.TLcdXYZPoint;
import com.luciad.transformation.TLcdGeoReference2GeoReference;
import com.luciad.util.TLcdConstant;
import com.luciad.util.TLcdOutOfBoundsException;

public class EulerAngleUtil {

	public static ILcdPoint getXYZ(ILcdPoint eyePoint,ILcdGeoReference worldReference){
		TLcdGeocentricReference ref = new TLcdGeocentricReference(new TLcdGeodeticDatum());

		 TLcdGeoReference2GeoReference geodetic2Topo = new TLcdGeoReference2GeoReference(new TLcdGeodeticReference(new TLcdGeodeticDatum()),
				 ref);

		    TLcdXYZPoint topocentricEyePoint = new TLcdXYZPoint();

		 try {
			 geodetic2Topo.sourcePoint2destinationSFCT(eyePoint, topocentricEyePoint);
		} catch (TLcdOutOfBoundsException e) {
			// TODO Auto-generated catch block
//			e.printStackTrace();
		}

		return topocentricEyePoint;
	}

	public static ILcdPoint getLatLonHeight(ILcdPoint eyePoint,ILcdGeoReference worldReference){
		TLcdGeocentricReference ref = new TLcdGeocentricReference(new TLcdGeodeticDatum());

		 TLcdGeoReference2GeoReference geodetic2Topo = new TLcdGeoReference2GeoReference(
				 ref,new TLcdGeodeticReference(new TLcdGeodeticDatum()));

		    TLcdXYZPoint topocentricEyePoint = new TLcdXYZPoint();

		 try {
			 geodetic2Topo.sourcePoint2destinationSFCT(eyePoint, topocentricEyePoint);
		} catch (TLcdOutOfBoundsException e) {
			// TODO Auto-generated catch block
//			e.printStackTrace();
		}

		return topocentricEyePoint;
	}
  public static double[] getEulerAngle(
      ILcdPoint eyePoint,
      ILcdPoint referencePoint,
      ILcdPoint upVector,
      ILcdGeoReference worldReference
  ) throws TLcdOutOfBoundsException {
    TLcdGeoReference2GeoReference world2Geodetic = new TLcdGeoReference2GeoReference(worldReference,
                                                                                     new TLcdGeodeticReference(worldReference.getGeodeticDatum()));

    TLcdLonLatHeightPoint geodeticPoint = new TLcdLonLatHeightPoint();
    world2Geodetic.sourcePoint2destinationSFCT(referencePoint, geodeticPoint);

    TLcdTopocentricReference topocentricReference = new TLcdTopocentricReference(worldReference.getGeodeticDatum(), geodeticPoint);

    TLcdGeoReference2GeoReference world2Topo = new TLcdGeoReference2GeoReference(worldReference
        , topocentricReference);

    TLcdXYZPoint topocentricEyePoint = new TLcdXYZPoint();
    TLcdXYZPoint topocentricReferencePoint = new TLcdXYZPoint();
    TLcdXYZPoint topocentricUpPoint = new TLcdXYZPoint(eyePoint);
    topocentricUpPoint.translate3D(
        upVector.getX(),
        upVector.getY(),
        upVector.getZ()
    );

    world2Topo.sourcePoint2destinationSFCT(eyePoint, topocentricEyePoint);
    world2Topo.sourcePoint2destinationSFCT(referencePoint, topocentricReferencePoint);
    world2Topo.sourcePoint2destinationSFCT(topocentricUpPoint, topocentricUpPoint);
    topocentricUpPoint.translate3D(
        -topocentricEyePoint.getX(),
        -topocentricEyePoint.getY(),
        -topocentricEyePoint.getZ()
    );

//
//		TLcdXYZPoint vector = new  TLcdXYZPoint(referencePoint.getX() - eyePoint.getX(),
//				referencePoint.getY() - eyePoint.getY(),
//				referencePoint.getZ() - eyePoint.getZ());
//
//		TLcdXYZPoint vectorP = new  TLcdXYZPoint(vector.getX(),
//				0,
//				vector.getZ());
//
//
//		TLcdXYZPoint vectorPP = new  TLcdXYZPoint(vector.getX(),
//				0,
//				vector.getZ());

    double dX = topocentricReferencePoint.getX() - topocentricEyePoint.getX();
    double dY = topocentricReferencePoint.getY() - topocentricEyePoint.getY();
    double dZ = topocentricReferencePoint.getZ() - topocentricEyePoint.getZ();

//		Vector3d original = new Vector3d(dX, dY, dZ);
//
//		Vector3d projectXY = new Vector3d(dX, dY, 0);
//		Vector3d projectY = new Vector3d(0, dY, 0);
//
//		double yaw = Math.acos(projectXY.dot(projectY)/ (projectXY.length() * projectY.length()));
//
//		Vector3d projectYZ = new Vector3d(0, dY, dZ);
//
//		double pitch = Math.acos(projectYZ.dot(projectY)/ (projectYZ.length() * projectY.length())) ;

    double yaw = Math.atan2(dX, dY);

    //double pitch = Math.atan2(Math.sqrt(dY * dY + dX * dX), dZ);
    double pitch = Math.atan2(dZ, Math.sqrt(dY * dY + dX * dX));

    //atan2(-X, sqrt(Y*Y + Z+Z))

    yaw *= TLcdConstant.RAD2DEG;
    //yaw = 90 - yaw;
    pitch *= TLcdConstant.RAD2DEG;
    //pitch -=  270;
//    System.out.println("yaw : " + yaw + "  pitch: " + pitch);

    Vector3d up = new Vector3d(
        topocentricUpPoint.getX(),
        topocentricUpPoint.getY(),
        topocentricUpPoint.getZ()
    );
    up.normalize();

    Vector3d fwd = new Vector3d(
        topocentricReferencePoint.getX() - topocentricEyePoint.getX(),
        topocentricReferencePoint.getY() - topocentricEyePoint.getY(),
        topocentricReferencePoint.getZ() - topocentricEyePoint.getZ()
    );
    fwd.normalize();

    Vector3d right = new Vector3d();
    right.cross(new Vector3d(0, 0, 1), fwd);
    right.normalize();

    Vector3d idealUp = new Vector3d();
    idealUp.cross(fwd, right);
    idealUp.normalize();

    double roll = Math.acos(up.dot(idealUp)) * TLcdConstant.RAD2DEG;

  //  System.out.println("roll = " + roll);

    return new double[]{yaw, pitch, roll};

  }
}
