jQuery(function ($) {
    //#main-slider
    $(function () {
        $('#main-slider .carousel').carousel({
            interval: 8000
        });
    });
    $('.centered').each(function (e) {
        $(this).css('margin-top', ($('#main-slider').height() - $(this).height()) / 2);
    });
    $(window).resize(function () {
        $('.centered').each(function (e) {
            $(this).css('margin-top', ($('#main-slider').height() - $(this).height()) / 2);
        });
    });
    //portfolio
    //$(window).load(function(){
    //	$portfolio_selectors = $('.portfolio-filter >li>a');
    //	if($portfolio_selectors!=='undefined'){
    //		$portfolio = $('.portfolio-items');
    //		$portfolio.isotope({
    //			itemSelector : 'li',
    //			layoutMode : 'fitRows'
    //		});
    //		$portfolio_selectors.on('click', function(){
    //			$portfolio_selectors.removeClass('active');
    //			$(this).addClass('active');
    //			var selector = $(this).attr('data-filter');
    //			$portfolio.isotope({ filter: selector });
    //			return false;
    //		});
    //	}
    //});
    //contact form
    var form = $('.contact-form');
    form.submit(function () {
        $this = $(this);
        $.post($(this).attr('action'), function (data) {
            $this.prev().text(data.message).fadeIn().delay(3000).fadeOut();
        }, 'json');
        return false;
    });
    //goto top
    $('.gototop').click(function (event) {
        event.preventDefault();
        $('html, body').animate({
            scrollTop: $("body").offset().top
        }, 500);
    });
    //Pretty Photo
    var photoControls = $("a[rel^='prettyPhoto']");
    if (photoControls.length > 0) {
        photoControls.prettyPhoto({
            social_tools: false
        });
    }
    if (window.ko) {
        ko.observableArray.fn.moveUp = function (item) {
            this.valueWillMutate();
            var index = this.indexOf(item);
            if (index > 0) {
                this()[index] = this()[index - 1];
                this()[index - 1] = item;
            }
            this.valueHasMutated();
        };
        ko.observableArray.fn.moveDown = function (item) {
            this.valueWillMutate();
            var index = this.indexOf(item);
            if (index < this().length - 1) {
                this()[index] = this()[index + 1];
                this()[index + 1] = item;
            }
            this.valueHasMutated();
        };
    }
});
function debounce(func, timeout) {
    var _this = this;
    if (timeout === void 0) { timeout = 300; }
    var timer;
    return function () {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i] = arguments[_i];
        }
        clearTimeout(timer);
        timer = setTimeout(function () { func.apply(_this, args); }, timeout);
    };
}
$(document).ready(function () {
    /***phone number format***/
    $(".phone-format").keyup(function (e) {
        $(this).attr('maxlength', '14');
        var cleanInput = function (value) {
            return value ? value.replace(/[^\d]/g, '') : '';
        }, format = function (value) {
            if (value === undefined) {
                value = '';
            }
            if (value.length > 0) {
                if (value.length > 6) {
                    return '(' + value.substr(0, 3) + ') ' + value.substr(3, 3) + '-' + value.substr(6);
                }
                else if (value.length > 3) {
                    return '(' + value.substr(0, 3) + ') ' + value.substr(3);
                }
                else {
                    return '(' + (value + '   ').substr(0, 3) + ')';
                }
            }
            return '';
        };
        var curval = $(this).val();
        $(this).val(format(cleanInput(curval)));
    });
});
//# sourceMappingURL=main.js.map