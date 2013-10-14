// http://127.0.0.1:8006/operate?id=3&args=nstlTest

phantom.casperPath = 'C:\\casper';
phantom.injectJs(phantom.casperPath + '\\bin\\bootstrap.js');
phantom.outputEncoding = "System";
phantom.scriptEncoding = "System";

var casper = require('casper').create();

casper.start('http://i.taobao.com/my_taobao.htm?spm=1.1000386.5982201.1.CwZwH5&jlogid=p23000054c0ddc&nekot=zs/Fo7XEs6TV9w==1379865669363', function () {

    this.evaluate(doSearch, input.searchKeyWord);
});

casper.then(function () {
