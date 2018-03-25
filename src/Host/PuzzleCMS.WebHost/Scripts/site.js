"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/*jslint node: true */
/*jshint strict:false */
/* jshint -W097 */
var Calculator = /** @class */ (function () {
    function Calculator() {
    }
    Calculator.prototype.add = function (a, b) {
        return a + b;
    };
    Calculator.prototype.subtract = function (a, b) {
        return a - b;
    };
    Calculator.prototype.multiply = function (a, b) {
        return a * b;
    };
    Calculator.prototype.divide = function (a, b) {
        this.checkDivideByZero(b);
        return a / b;
    };
    Calculator.prototype.checkDivideByZero = function (x) {
        if ((x === 0) && this.throwOnDivideByZero) {
            throw new Error("Divide by Zero.");
        }
    };
    return Calculator;
}());
exports.Calculator = Calculator;
//# sourceMappingURL=site.js.map