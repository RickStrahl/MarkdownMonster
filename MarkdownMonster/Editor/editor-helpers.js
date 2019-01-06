// Global functions and helpers

function debounce(func, wait, immediate) {
  var timeout;
  return function () {
    var context = this, args = arguments;
    var later = function () {
      timeout = null;
      if (!immediate) func.apply(context, args);
    };
    var callNow = immediate && !timeout;
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
    if (callNow)
      func.apply(context, args);
  };
};

String.prototype.startsWith = function (sub, nocase) {
  if (!this || this.length === 0 || sub === null) return false;

  if (sub && nocase)
    return this.toLowerCase().indexOf(sub.toLowerCase()) === 0;

  return this.indexOf(sub) === 0;
}
String.prototype.endsWith = function (sub, nocase) {
  if (!this || this.length === 0) return false;

  var ix = 0;
  if (sub && nocase) {
    ix = this.toLowerCase().lastIndexOf(sub.toLowerCase());
    if (ix > 0 && ix + sub.length === this.length)
      return true;
    return false;
  }

  ix = this.lastIndexOf(sub);
  if (ix > 0 && ix + sub.length === this.length)
    return true;
  return false;
}
String.prototype.extract = function (startDelim, endDelim, allowMissingEndDelim, returnDelims) {
  var str = this;
  if (str.length === 0)
    return "";

  var src = str.toLowerCase();
  startDelim = startDelim.toLocaleLowerCase();
  endDelim = endDelim.toLocaleLowerCase();

  var i1 = src.indexOf(startDelim);
  if (i1 == -1)
    return "";

  var i2 = src.indexOf(endDelim, i1 + startDelim.length);

  if (!allowMissingEndDelim && i2 == -1)
    return "";

  if (allowMissingEndDelim && i2 == -1) {
    if (returnDelims)
      return str.substr(i1);

    return str.substr(i1 + startDelim.length);
  }

  if (returnDelims)
    return str.substr(i1, i2 - i1 + startDelim.length);

  return str.substr(i1 + startDelim.length, i2 - i1 - startDelim.length);
};
