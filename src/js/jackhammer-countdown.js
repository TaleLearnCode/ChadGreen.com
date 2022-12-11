//var endDate = new Date("October 31, 2014 20:00:00");
//Set line 4 to your estimated end date (GMT timezone)

var endDate = new Date("October 31, 2015 20:00:00");
var daysLeft;
//var endDate = new Date(+new Date + 25096e5);

window.addEventListener('load', function () {
	
	iniJackhammer();
}, false);

//page loaded
function iniJackhammer() 
{

	daysLeft = document.getElementById("daysLeft");

	ShowTimeLeft();
	window.setInterval(CallEverySecond, 1000);

}

function CallEverySecond()
{
	ShowTimeLeft();
}

function ShowTimeLeft()
{
	
	var now = new Date();
	var nowUtc = new Date( now.getTime() + (now.getTimezoneOffset() * 60000));

	var msDiff = (endDate -  nowUtc)/1000;

	// calculate (and subtract) whole days
	var days = Math.floor(msDiff / 86400);
	msDiff -= days * 86400;

	// calculate (and subtract) whole hours
	var hours = Math.floor(msDiff / 3600) % 24;
	msDiff -= hours * 3600;

	// calculate (and subtract) whole minutes
	var minutes = Math.floor(msDiff / 60) % 60;
	msDiff -= minutes * 60;

	// what's left is seconds
	var seconds = Math.floor(msDiff % 60); 


	daysLeft.textContent = days;
	hoursLeft.textContent = hours;
	minutesLeft.textContent = minutes;
	secondsLeft.textContent = seconds;

}

