
// http://127.0.0.1:8006/operate?id=2&args=android

var args = require('system').args;
if (args.length !== 3) {
    console.log(args.length);
    console.log('you must run with 3 Args');
    phantom.exit();
}

var sessionId = args[1];
var searchKeyWords = args[2];

console.log('sessionId:' + sessionId);
console.log('searchKeyWords:' + searchKeyWords);

phantom.casperPath = 'C:\\casper';
phantom.injectJs(phantom.casperPath + '\\bin\\bootstrap.js');

//var casper = require('casper').create({
//    clientScripts: ["jquery-1.9.1.min.js"]
//});
var casper = require('casper').create();

casper.start('http://www.sciencedirect.com/');

casper.then(function () {
    this.echo('http://www.sciencedirect.com/' + 'loaded');
    this.evaluate(function () {
        $("#qs_all").attr("value", searchKeyWords);
        $("#submit_search").trigger("click");
    });
});

casper.then(function () {
    this.echo('search submitd');
    var bibCount = this.evaluate(function () {
        return $(".iconLinks strong").first().text();
    });
    this.echo("bibCount:" + bibCount);
    casper.open('http://127.0.0.1:8006/callback?sessionId=' + sessionId, {
        method: 'post',
        data: {
            'count': bibCount
        }
    });
});

casper.run();