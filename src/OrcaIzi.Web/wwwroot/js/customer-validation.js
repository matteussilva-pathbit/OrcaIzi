$(document).ready(function () {
    // Máscaras
    $('#Customer_Document').on('input', function() {
        var value = $(this).val().replace(/\D/g, '');
        if (value.length <= 11) {
            // CPF
            value = value.replace(/(\d{3})(\d)/, '$1.$2');
            value = value.replace(/(\d{3})(\d)/, '$1.$2');
            value = value.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
        } else {
            // CNPJ
            value = value.replace(/^(\d{2})(\d)/, '$1.$2');
            value = value.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3');
            value = value.replace(/\.(\d{3})(\d)/, '.$1/$2');
            value = value.replace(/(\d{4})(\d)/, '$1-$2');
        }
        $(this).val(value);
    });

    // Feedback visual para o documento
    function showDocumentFeedback(message, isError) {
        $('#document-feedback').remove();
        var color = isError ? '#FF4444' : '#00C851';
        var icon = isError ? 'bi-exclamation-circle' : 'bi-check-circle';
        $('#Customer_Document').after('<div id="document-feedback" class="small mt-1" style="color: ' + color + ' !important;"><i class="bi ' + icon + ' me-1"></i>' + message + '</div>');
    }

    // Consulta CNPJ / Validação CPF
    $('#Customer_Document').on('blur', function () {
        var doc = $(this).val().replace(/\D/g, '');
        
        if (doc.length === 14) {
            // É CNPJ, consultar API
            showDocumentFeedback('Consultando CNPJ...', false);
            $.get('?handler=ConsultarCnpj&cnpj=' + doc, function (data) {
                if (data && data.razao_Social) {
                    showDocumentFeedback('CNPJ encontrado!', false);
                    if (!$('#Customer_Name').val()) $('#Customer_Name').val(data.razao_Social);
                    if (!$('#Customer_Phone').val()) $('#Customer_Phone').val(data.ddd_Telefone_1);
                    
                    var endereco = '';
                    if (data.logradouro) endereco += data.logradouro;
                    if (data.numero) endereco += ', ' + data.numero;
                    if (data.bairro) endereco += ' - ' + data.bairro;
                    if (data.municipio) endereco += ' - ' + data.municipio;
                    if (data.uf) endereco += '/' + data.uf;
                    if (data.cep) endereco += ' CEP: ' + data.cep;
                    
                    if (!$('#Customer_Address').val()) $('#Customer_Address').val(endereco);
                } else {
                    showDocumentFeedback('CNPJ não encontrado ou inválido.', true);
                }
            }).fail(function() {
                showDocumentFeedback('Erro ao consultar CNPJ.', true);
            });
        } else if (doc.length === 11) {
            // É CPF, validar
            $.get('?handler=ConsultarCpf&cpf=' + doc, function (data) {
                if (data && data.valido) {
                    showDocumentFeedback('CPF Válido (' + data.regiaoFiscal + ')', false);
                } else {
                    showDocumentFeedback('CPF Inválido.', true);
                }
            });
        }
    });
});
