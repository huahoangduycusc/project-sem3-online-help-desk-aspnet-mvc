const input = document.querySelectorAll('input');
input.forEach(item => {
    item.addEventListener('focus',(e) => {
       item.offsetParent.classList.add('active');
    });
    item.addEventListener('blur',(e) => {
        if(e.target.value == ""){
            item.offsetParent.classList.remove('active');
        }
    });
	console.log(input.value);
});
// forgot password
var click = document.querySelector("#forgot");
var feedback = document.querySelector(".feedback-container");
click.addEventListener('click',(e) => {
    e.preventDefault();
    feedback.classList.toggle("actives");
});
var fb = document.querySelector(".feedback-fixed");
feedback.addEventListener('click',function(e){
    var div = e.target;
    if(div.classList.contains('feedback-container'))
    {
        feedback.classList.remove("actives");
    }
});
var fb_close = document.querySelector('.close');
if(fb_close){
    fb_close.addEventListener('click',function(e){
        e.preventDefault();
        feedback.classList.remove("actives");
    });
}