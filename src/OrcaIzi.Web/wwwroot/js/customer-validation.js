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
        // Ao enviar o formulário, concatena endereço, número e complemento
    $('form').submit(function() {
        var endereco = $('#enderecoInput').val();
        var numero = $('#numeroInput').val();
        var complemento = $('#complementoInput').val();

        if (endereco && numero) {
            var enderecoCompleto = endereco;
            // Se o endereço já não tiver o número (caso de edição manual)
            if (endereco.indexOf(', ' + numero) === -1) {
                enderecoCompleto += ', ' + numero;
            }
            
            if (complemento) {
                enderecoCompleto += ' - ' + complemento;
            }
            
            $('#enderecoInput').val(enderecoCompleto);
        }
    });

    $('#btnBuscarCep').click(function() {
        var cep = $('#cepInput').val().replace(/\D/g, '');
        if (cep != "") {
            var validacep = /^[0-9]{8}$/;
            if(validacep.test(cep)) {
                // Loading
                $('#enderecoInput').val("...");

                $.getJSON("https://viacep.com.br/ws/" + cep + "/json/?callback=?", function(dados) {
                    if (!("erro" in dados)) {
                        // Atualiza os campos com os valores da consulta.
                        // Mantém separado para o usuário conferir
                        var enderecoBase = dados.logradouro + " - " + dados.bairro + ", " + dados.localidade + "/" + dados.uf;
                        $('#enderecoInput').val(enderecoBase);
                        $('#numeroInput').focus();
                    } else {
                        alert("CEP não encontrado.");
                        $('#enderecoInput').val("");
                    }
                });
            } else {
                alert("Formato de CEP inválido.");
            }
        }
    });

    // Also trigger on blur if valid length
    $('#cepInput').blur(function() {
        var cep = $(this).val().replace(/\D/g, '');
        if (cep.length === 8) {
            $('#btnBuscarCep').click();
        }
    });
});

    // Consulta CNPJ / Validação CPF
    $('#Customer_Document').on('blur', function () {
        var doc = $(this).val().replace(/\D/g, '');
        
        if (doc.length === 14) {
            // É CNPJ, consultar API
            $('#document-feedback').remove();
            $('#Customer_Document').after('<div id="document-feedback" class="small mt-1 text-info"><i class="bi bi-arrow-clockwise me-1"></i>Consultando CNPJ...</div>');
            
            $.get('?handler=ConsultarCnpj&cnpj=' + doc, function (data) {
                if (data && data.razao_Social) {
                    showDocumentFeedback('CNPJ encontrado!', false);
                    if (!$('#Customer_Name').val()) $('#Customer_Name').val(data.razao_Social);
                    
                    // Phone logic (ddd_Telefone_1 might be complex object or string, simplified here)
                    if (!$('#Customer_Phone').val() && data.ddd_Telefone_1) $('#Customer_Phone').val(data.ddd_Telefone_1);
                    
                    var endereco = '';
                    if (data.logradouro) endereco += data.logradouro;
                    if (data.numero) endereco += ', ' + data.numero;
                    if (data.bairro) endereco += ' - ' + data.bairro;
                    if (data.municipio) endereco += ' - ' + data.municipio;
                    if (data.uf) endereco += '/' + data.uf;
                    
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

function showDocumentFeedback(message, isError) {
    $('#document-feedback').remove();
    var color = isError ? '#FF4444' : '#00C851';
    var icon = isError ? 'bi-exclamation-circle' : 'bi-check-circle';
    $('#Customer_Document').after('<div id="document-feedback" class="small mt-1" style="color: ' + color + ' !important;"><i class="bi ' + icon + ' me-1"></i>' + message + '</div>');
}
