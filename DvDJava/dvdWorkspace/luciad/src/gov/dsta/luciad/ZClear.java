package gov.dsta.luciad;

import com.luciad.model.TLcdVectorModel;
import com.luciad.view.lightspeed.ILspView;
import com.luciad.view.lightspeed.TLspOpenGLProfile;
import com.luciad.view.lightspeed.TLspPaintPhase;
import com.luciad.view.lightspeed.TLspPaintProgress;
import com.luciad.view.lightspeed.layer.ALspLayer;
import com.luciad.view.lightspeed.layer.ILspPaintableLayer;
import com.luciad.view.lightspeed.layer.TLspPaintRepresentation;
import com.luciad.view.lightspeed.layer.TLspPaintRepresentationState;
import com.luciad.view.opengl.binding.ILcdGL;
import com.luciad.view.opengl.binding.ILcdGLDrawable;
/*
 * A hack created to hide the map layer and show only the overlay.
 */
public class ZClear extends ALspLayer implements ILspPaintableLayer {

	  public ZClear() {
	    super("Z clear");
	    setModel(new TLcdVectorModel());
	    addPaintRepresentation(TLspPaintRepresentation.BODY);

	    setSelectable(false);
	    setEditable(false);
	    setVisible(true);
	  }

	  @Override
	  public TLspOpenGLProfile getRequiredOpenGLProfile() {
	    return TLspOpenGLProfile.LIGHTSPEED_MINIMUM;
	  }

	  public TLspPaintProgress paint(ILcdGLDrawable aGLDrawable, TLspPaintPhase aPhase, TLspPaintRepresentationState aPaintRepresentationState, ILspView aView) {
	    if (!isVisible(aPaintRepresentationState)) {
	      return TLspPaintProgress.COMPLETE;
	    }
	    if (aPhase.getPaintOpacity() == TLspPaintPhase.PaintOpacity.TRANSPARENT) {
	      return TLspPaintProgress.COMPLETE;
	    }
	    if (aPhase.getPaintDraping() == TLspPaintPhase.PaintDraping.DRAPING) {
	      return TLspPaintProgress.COMPLETE;
	    }

	    aGLDrawable.getGL().glClear(ILcdGL.GL_DEPTH_BUFFER_BIT);

	    return TLspPaintProgress.COMPLETE;
	  }

	}

