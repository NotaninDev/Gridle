// This code is a modified version of Zeno Rocha's code
// which is licensed under MIT License
// For more infomation see clipboardjs_LICENSE.txt
// and https://clipboardjs.com

mergeInto(LibraryManager.library, {

    CopyToClipboard__postset: '_CopyToClipboard();',

    CopyToClipboard: function(){
        function command(type) {
            try {
                return document.execCommand(type);
            } catch (err) {
                return false;
            }
        }

        function createFakeElement(value) {
            var isRTL = document.documentElement.getAttribute('dir') === 'rtl';
            var fakeElement = document.createElement('textarea'); // Prevent zooming on iOS

            fakeElement.style.fontSize = '12pt'; // Reset box model

            fakeElement.style.border = '0';
            fakeElement.style.padding = '0';
            fakeElement.style.margin = '0'; // Move element out of screen horizontally

            fakeElement.style.position = 'absolute';
            fakeElement.style[isRTL ? 'right' : 'left'] = '0px'; // Move element to the same position vertically

            var yPosition = window.pageYOffset || document.documentElement.scrollTop;
            fakeElement.style.top = "".concat(yPosition, "px");
            fakeElement.setAttribute('readonly', '');
            fakeElement.value = value;
            return fakeElement;
        }

        function selectTextArea(element) {
            if (element.nodeName === 'SELECT') {
                element.focus();
            }
            else if (element.nodeName === 'INPUT' || element.nodeName === 'TEXTAREA') {
                var isReadOnly = element.hasAttribute('readonly');

                if (!isReadOnly) {
                    element.setAttribute('readonly', '');
                }

                element.focus();
                element.select();
                element.setSelectionRange(0, element.value.length);

                if (!isReadOnly) {
                    element.removeAttribute('readonly');
                }
            }
            else {
                if ((typeof element.hasAttribute) === 'function' && element.hasAttribute('contenteditable')) {
                    element.focus();
                }

                var selection = window.getSelection();
                var range = document.createRange();

                range.selectNodeContents(element);
                selection.removeAllRanges();
                selection.addRange(range);
            }
        }

        _CopyToClipboard = function(str) {
            var options = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : {
                container: document.body
            };
            var copiedText = Pointer_stringify(str);
            
            var fakeElement = createFakeElement(copiedText);
            options.container.appendChild(fakeElement);

            // copy
            selectedText = selectTextArea(fakeElement);
            var copyResult = command('copy');

            fakeElement.remove();
            return copyResult;
        };
    }
});