/**
 * 
 */

var frameNumber = 0;

window.addEventListener('load', function () {
	initJackhammer();
	animateIn();
}, false);


function initJackhammer()
{
	var jackhammerTip = document.getElementById("jackhammerTip");
	var jackhammer = document.getElementById("jackhammer");
	var jackBody = document.getElementById("jackBody");
	var jackArms = document.getElementById("jackArms");
	setInterval(animateJackhammer, 40);

	
	
	
}

function animateJackhammer()
{
	frameNumber++;
	jackhammerTip.setAttribute('transform', 'translate(0,'+frameNumber%3 * -3+')');
	
	jackhammer.setAttribute('transform', 'translate(0,'+ -Math.sin(frameNumber*1.5) +')');
	
	var jackBodyAmount = Math.sin(frameNumber) + 2;
	jackBody.setAttribute('transform', 'translate(0,'+ -jackBodyAmount +')');
	jackArms.setAttribute('transform', 'translate(0,'+ -jackBodyAmount +')');
	
	if(frameNumber == 1000)
		frameNumber = 0;
}


function animateIn()
{

	var tweenTime = .5;
	var fromBottom = 12;

	
	TweenMax.to(jackhammerSVG, 1, {delay:.2, opacity:1});
	TweenMax.from(jackhammer_title, 1, {alpha:0});
	TweenMax.from(jackhammer_socialmedia, 1, {alpha:0});

	var jackhammerCountdown = document.getElementById("svg_countdown");

	if(jackhammerCountdown){

		TweenMax.to(jackhammer_cube1, tweenTime, {delay:.2, y:-fromBottom, opacity:1});
		TweenMax.to(jackhammer_cube2, tweenTime, {delay:.3, y:-fromBottom, opacity:1});
		TweenMax.to(jackhammer_cube3, tweenTime, {delay:.4, y:-fromBottom, opacity:1});
		TweenMax.to(jackhammer_cube4, tweenTime, {delay:.5, y:-fromBottom, opacity:1});
		TweenMax.to(countdownshadow, tweenTime, {delay:.8, opacity:1});
	}
}