; (function ($, window, undefined) {

    //Defaults
    var defaults = {
        // fileTypes: ['pdf']
    };

    // Constructor
    function fileUpload(element, options) {
        // Assign the DOM element
        this.element = $(element);
        this.options = $.extend({}, defaults, options);
        this.defaults = defaults;
        // Initialise the select
        this.initialise();
    }

    // Initialise
    fileUpload.prototype.initialise = function () {
        var self = this;
        // Declare the main UI components
        this.body = $(this.element).children('tbody').first();
        this.newRow = $(this.element).children('tbody').last().children('tr');
        this.footer = $(this.element).children('tfoot').children('tr');
        //this.fileInput = footer.find('input[type="file"]');
        this.addButton = this.footer.find('.add-button');
        this.form = $(this.element).closest('form');
        if (!this.form.attr('enctype')) {
            this.form.attr('enctype', 'multipart/form-data');
        }

        // **************Events****************

        // Display the file selection dialog
        this.addButton.click(function (e) {
            self.showDialog();
        });

        // Delete new rows from the table or archive/activate existing rows
        this.body.on('click', '.table-button', function () {
            self.deleteRow($(this));
        })

        // Add new table row
        this.footer.on('change', 'input', function () {
            self.addRow($(this));
        })

        // Remove the new row inputs so they are not sent in the request
        this.form.submit(function () {
            self.newRow.find('input').remove();
            self.footer.find('input').remove();
        })
    }

    // Opens the file dialog
    fileUpload.prototype.showDialog = function () {
        // Open the dialog
        this.footer.find('input').trigger('click');
    }

    // Adds a new row to the table
    fileUpload.prototype.addRow = function (fileInput) {
        // Get the selected file
        var file = fileInput[0].files[0];
        // TODO: Validate (file size, file type (extension), duplicate names?)

        // Clone the new row
        var newRow = this.newRow.clone();
        // Update the indexer
        var index = $.now();
        var indexerInput = newRow.find('input').last();
        indexerInput.val(index);
        // Update the display values
        var cells = newRow.find('.table-text');
        cells.eq(0).text(file.name);
        cells.eq(1).text(Math.round(file.size / 1024) + ' KB');
        // Clone the file input and add to the DOM
        var clone = fileInput.clone();
        clone.val('');
        fileInput.after(clone);
        // Rename the file input and move it to the new row
        var name = fileInput.attr('name');
        fileInput.attr('name', name.replace('#', index));
        indexerInput.after(fileInput);
        // Rename other inputs
        var inputs = newRow.find('.table-control');
        $.each(inputs, function () {
            name = $(this).attr('name').replace('#', index);
            $(this).attr('name', name);
            if ($(this).is(':checkbox')) {
                $(this).next().attr('name', name);
            }
            // Rename validation message element
            $(this).siblings('span[data-valmsg-for]').attr('data-valmsg-for', name);
        })
        // Add new row
        this.body.append(newRow);

        // Reparse the validator
        // TODO: Get the unobtrusive validation - this.form.data(unobtrusiveValidation);
        // and add the rules for the new elements to save reparsing the whole form
        // https://xhalent.wordpress.com/2011/01/24/applying-unobtrusive-validation-to-dynamic-content/
        if ($.validator) {
            this.form.data('validator', null);
            $.validator.unobtrusive.parse(this.form)
        }
        inputs.first().focus();
    }

    // Delete new rows from the table or archive/activate existing rows
    fileUpload.prototype.deleteRow = function (button) {
        var row = button.closest('tr');
        var input = row.find('.file-status');
        if (input.length === 0) {
            // It never existed so remove from DOM
            row.remove();
            //TODO: What should we set focus to?
            this.addButton.focus();
        } else if (row.hasClass('archived')) {
            // Un-mark it for deletion
            row.removeClass('archived');
            input.val(1);

        } else {
            // Mark it for deletion
            row.addClass('archived');
            input.val(-1);
        }
    }

    // fileUpload definition
    $.fn.fileUpload = function (options) {
        return this.each(function () {
            if (!$.data(this, 'fileUpload')) {
                $.data(this, 'fileUpload', new fileUpload(this, options));
            }
        });
    }

}(jQuery, window));