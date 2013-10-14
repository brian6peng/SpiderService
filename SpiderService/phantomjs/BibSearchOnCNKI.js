// http://127.0.0.1:8006/operate?id=1&args=android

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
phantom.outputEncoding = "System";


var casper = require('casper').create();

casper.start('http://cnki.net/');

casper.then(function () {
    this.echo('http://cnki.net/' + 'loaded');
    this.evaluate(function () {
        $("#txt_1_value1").attr("value", "android");
        $("#btnSearch").trigger("click");
    });
});

casper.then(function () {
    this.echo('search submitd');
    this.echo(this.getTitle());
    casper.withFrame('iframeResult', function () {
        this.echo('switch to frame');
        //        this.echo(this.getTitle());
        //        this.echo(this.fetchText('span[name="lbPagerTitle"]'));
        var bibCount = this.evaluate(function () {
            return $('.pagerTitleCell').html();
        });
        this.echo(bibCount);
        this.click('input[name="selectCheckbox"]');
        this.clickLabel(' 导出 / 参考文献');
    });
});

casper.then(function () {
    this.echo('http://cnki.net/' + 'export');
    this.capture('cnki.png');
});

casper.run();