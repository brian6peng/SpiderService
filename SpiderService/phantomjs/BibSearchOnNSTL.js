// http://127.0.0.1:8006/operate?id=3&args=nstlTest

phantom.casperPath = 'C:\\casper';
phantom.injectJs(phantom.casperPath + '\\bin\\bootstrap.js');
phantom.outputEncoding = "System";
phantom.scriptEncoding = "System";

var args = require('system').args;
if (args.length !== 2) {
    console.log(args.length);
    console.log('you must run with 3 Args');
    phantom.exit();
}

var id = args[0];
var inputPath = args[1];

console.log('id: ' + id);
console.log('inputPath: ' + inputPath);




//var input = {

//    searchKeyWord: 'computer',
//    outputPath: 'nstlTest'
//};

//read input 
var fs = require('fs');
var inputText = fs.read(inputPath);
console.log('inputText: ' + inputText);
var input = JSON.parse(inputText);

var result = null;
var bibs = [];

function doSearch(keyword) {

    document.getElementById('sf1').options[1].selected = true;
    document.getElementsByName('kw1')[0].value = keyword;
    validate();
}

function extraction() {

    var records = document.getElementById('records').children;
    var bibArr = [];
    var i = 0;
    while (i < records.length) {

        bibArr[i] = {
            id: records[i].id.substring(7, records[i].id.length),
            title: records[i].children[0].children[1].children[0].innerText,
            author: records[i].children[0].children[1].children[2].innerText,
            other: records[i].children[0].children[1].innerText
        };
        i++;
        if (i >= 10) {
            break;
        }
    }
    return bibArr;
}

var casper = require('casper').create();

casper.start('http://www.nstl.gov.cn/NSTL/facade/search/searchByDocType.do?subDocTypes=Z01,J02,J03,J04&name_chi=%D1%A7%CA%F5%C6%DA%BF%AF', function () {

    this.evaluate(doSearch, input.searchKeyWord);
});

casper.then(function () {

    result = { resultCount: this.fetchText('#hitcount1'), bibs: [] };
    this.echo('hit count: ' + result.resultCount);
    bibs = this.evaluate(extraction);
    result.bibs = bibs;

    //write output
    console.log('write path: ' + input.outputPath);
    console.log('write content: ' + JSON.stringify(result, undefined, 4));
    var fsStream = fs.open(input.outputPath, 'a');
    fsStream.write(JSON.stringify(result, undefined, 4));
    fsStream.flush();
    fsStream.close();
})

casper.run();