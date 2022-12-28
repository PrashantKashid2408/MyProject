var initialized = false;
var faceSpotted = false;
var startVisible = false;
var noFaceTimeout;
function button_callback() {
	/*
		(0) check whether we're already running face detection
	*/
	if(initialized)
		return; // if yes, then do not initialize everything again
	/*
		(1) prepare the pico.js face detector
	*/
	var update_memory = pico.instantiate_detection_memory(5); // we will use the detecions of the last 5 frames
	var facefinder_classify_region = function(r, c, s, pixels, ldim) {return -1.0;};
	var cascadeurl = 'https://raw.githubusercontent.com/nenadmarkus/pico/c2e81f9d23cc11d1a612fd21e4f9de0921a5d0d9/rnt/cascades/facefinder';
	fetch(cascadeurl).then(function(response) {
		response.arrayBuffer().then(function(buffer) {
			var bytes = new Int8Array(buffer);
			facefinder_classify_region = pico.unpack_cascade(bytes);
			console.log('* cascade loaded');
			//$("#start-call-button").click();
			//$("#cnFaceDetect").hide();
		})
	})
	/*
		(2) get the drawing context on the canvas and define a function to transform an RGBA image to grayscale
	*/
	var ctx = document.getElementsByTagName('canvas')[0].getContext('2d');
	function rgba_to_grayscale(rgba, nrows, ncols) {
		var gray = new Uint8Array(nrows*ncols);
		for(var r=0; r<nrows; ++r)
			for(var c=0; c<ncols; ++c)
				// gray = 0.2*red + 0.7*green + 0.1*blue
				gray[r*ncols + c] = (2*rgba[r*4*ncols+4*c+0]+7*rgba[r*4*ncols+4*c+1]+1*rgba[r*4*ncols+4*c+2])/10;
		return gray;
	}
	/*
		(3) this function is called each time a video frame becomes available
	*/
	var processfn = function (video, dt) {
		// render the video frame to the canvas element and extract RGBA pixel data
		ctx.drawImage(video, 0, 0);
		var rgba = ctx.getImageData(0, 0, 640, 480).data;
		// prepare input to `run_cascade`
		image = {
			"pixels": rgba_to_grayscale(rgba, 480, 640),
			"nrows": 480,
			"ncols": 640,
			"ldim": 640
		}
		params = {
			"shiftfactor": 0.1, // move the detection window by 10% of its size
			"minsize": 100,     // minimum size of a face
			"maxsize": 1000,    // maximum size of a face
			"scalefactor": 1.1  // for multiscale processing: resize the detection window by 10% when moving to the higher scale
		}
		// run the cascade over the frame and cluster the obtained detections
		// dets is an array that contains (r, c, s, q) quadruplets
		// (representing row, column, scale and detection score)
		dets = pico.run_cascade(image, facefinder_classify_region, params);
		dets = update_memory(dets);
		dets = pico.cluster_detections(dets, 0.2); // set IoU threshold to 0.2
		// draw detections
		
		
		for (i = 0; i < dets.length; ++i)
			// check the detection score
			// if it's above the threshold, draw it
			// (the constant 50.0 is empirical: other cascades might require a different one)
			if (dets[i][3] > 50.0) {
				//ClearNoFaceTimeOut()
				ctx.beginPath();
				ctx.arc(dets[i][1], dets[i][0], dets[i][2] / 2, 0, 2 * Math.PI, false);
				ctx.lineWidth = 3;
				ctx.strokeStyle = 'red';
				ctx.stroke();
				if (!faceSpotted && !wasCallConnected) {
					//$("#user-access-token").val("eyJhbGciOiJSUzI1NiIsImtpZCI6IjEwMyIsIng1dCI6Ikc5WVVVTFMwdlpLQTJUNjFGM1dzYWdCdmFMbyIsInR5cCI6IkpXVCJ9.eyJza3lwZWlkIjoiYWNzOmFkOWNkYWRiLTY2YTUtNDVhMi04NTdmLTgwMmY0NzgwNWUzYl8wMDAwMDAwZC00NmFlLWRhYjQtNjdiMC05ZjNhMGQwMDAyYzEiLCJzY3AiOjE3OTIsImNzaSI6IjE2MzQ4ODYwMzkiLCJleHAiOjE2MzQ5NzI0MzksImFjc1Njb3BlIjoidm9pcCIsInJlc291cmNlSWQiOiJhZDljZGFkYi02NmE1LTQ1YTItODU3Zi04MDJmNDc4MDVlM2IiLCJpYXQiOjE2MzQ4ODYwMzl9.qjAB-2RxLN8bURT0wj66JUmzU1ArqLcHBP-8uBxWBrKldNo2aMTLbXPYCoHvWAG0Xsu2XnObgfwsqQRBtKiadbqxIj17XjxMToKMEfLXM5jQ9dc4JOLEzT8Eh1ke7zcCANbtDRhVjOPSV7g9Txl5MInCFpVAxL9nYi-Cd0Dj3mdLoO-uZVjpSUJzpcng686HJalhyK93Bnybu2-tSNWr1f_2CjlXe-koyVmehUyTBa9qXWpLZq-lWZyV4MOYfSnQ99sx3wCHlt--wZPTQs4XxSrAOj0gqPPh4v47FNawGP5uPNio0etq6ay-nyfr3kYhKf12vBu6Qeocd5zae3CzRA");
					//$("#callee-acs-user-id").val("8:acs:ad9cdadb-66a5-45a2-857f-802f47805e3b_0000000d-46af-8ccd-81b8-9f3a0d0002a6");
					$("#callee-acs-user-id").val("")
					GetAvailableAgent('face')
					if ($("#callee-acs-user-id").val() != '') {
						faceSpotted = true;
						$("#initialize-call-agent").click();
						startVisible = false;//click start if start is enabled.
						console.log("this is face spotted old:")
					}
					//CheckStart();
				}
			}
			//else if (dets[i][3] < 1.0) {
			//	noFaceTimeout = setTimeout(function () {
			//		ShowDefaultScreenAfterWait()
			//	}, 240000);
			//}
	}
	/*
		(4) instantiate camera handling (see https://github.com/cbrandolino/camvas)
	*/
	var mycamvas = new camvas(ctx, processfn);
	/*
		(5) it seems that everything went well
	*/
	initialized = true;

}

function CheckStart()
{
	LoadCall();
	if (thankyouStatus == 0) {
		if (LoadCallCount <= LoadCountAllowed) {
			if (!startVisible && $("#remoteVideoContainer").html() == '') {
				if ($("#start-call-button").prop("disabled") == false && $("#callee-acs-user-id").val() != "") {//Start button is visible
					startVisible = true;
					if ($('#start-call-button').hasClass("btn-disable")) {
						$('#start-call-button').removeClass("btn-disable")
						$('#start-call-button').prop("disabled", false)
					}
					$("#start-call-button").click();
					$('#start-call-button').addClass("btn-disable")
					$('#start-call-button').prop("disabled", true)
					$("#cnFaceDetect").hide();
					console.log("this is CheckStart():")
				}
			}
		}

		//if ($("#connectedLabel").is(":hidden") == true && $('#start-call-button').hasClass("btn-disable") == false) {
		//	faceSpotted = false;
		//}
	}
}

//setTimeout(button_callback,20000);

setInterval(CheckStart, 5000);

function LoadCall() {
	if (LoadCallCount > LoadCountAllowed) {
		//window.close();
		if (LoadCallCount > LoadCountAllowed) {
			//history.go(-2);
			FreeAgent($("#callee-acs-user-id").val(), '11indexLoadCall')
			setTimeout(function () {
				window.location.href = '/Landing/Index';
			}, 5000);
		}
	}
	else {
		console.log("this is LoadCall() :" + faceSpotted + ",wasCallConnected:" + wasCallConnected )
		if (!faceSpotted && !wasCallConnected) {
			//$("#user-access-token").val("eyJhbGciOiJSUzI1NiIsImtpZCI6IjEwMyIsIng1dCI6Ikc5WVVVTFMwdlpLQTJUNjFGM1dzYWdCdmFMbyIsInR5cCI6IkpXVCJ9.eyJza3lwZWlkIjoiYWNzOmFkOWNkYWRiLTY2YTUtNDVhMi04NTdmLTgwMmY0NzgwNWUzYl8wMDAwMDAwZC00NmFlLWRhYjQtNjdiMC05ZjNhMGQwMDAyYzEiLCJzY3AiOjE3OTIsImNzaSI6IjE2MzQ4ODYwMzkiLCJleHAiOjE2MzQ5NzI0MzksImFjc1Njb3BlIjoidm9pcCIsInJlc291cmNlSWQiOiJhZDljZGFkYi02NmE1LTQ1YTItODU3Zi04MDJmNDc4MDVlM2IiLCJpYXQiOjE2MzQ4ODYwMzl9.qjAB-2RxLN8bURT0wj66JUmzU1ArqLcHBP-8uBxWBrKldNo2aMTLbXPYCoHvWAG0Xsu2XnObgfwsqQRBtKiadbqxIj17XjxMToKMEfLXM5jQ9dc4JOLEzT8Eh1ke7zcCANbtDRhVjOPSV7g9Txl5MInCFpVAxL9nYi-Cd0Dj3mdLoO-uZVjpSUJzpcng686HJalhyK93Bnybu2-tSNWr1f_2CjlXe-koyVmehUyTBa9qXWpLZq-lWZyV4MOYfSnQ99sx3wCHlt--wZPTQs4XxSrAOj0gqPPh4v47FNawGP5uPNio0etq6ay-nyfr3kYhKf12vBu6Qeocd5zae3CzRA");
			//$("#callee-acs-user-id").val("8:acs:ad9cdadb-66a5-45a2-857f-802f47805e3b_0000000d-46af-8ccd-81b8-9f3a0d0002a6");
			$("#callee-acs-user-id").val("")
			GetAvailableAgent('face')
			if ($("#callee-acs-user-id").val() != '') {
				faceSpotted = true;
				document.getElementById('initialize-call-agent').disabled = false;
				$("#initialize-call-agent").click();
				document.getElementById('initialize-call-agent').disabled = true;
				startVisible = false;//click start if start is enabled.
				console.log("this is face spotted:")
				LoadCallCount = LoadCallCount + 1;
			}
			//CheckStart();
		}
    }
	
}


