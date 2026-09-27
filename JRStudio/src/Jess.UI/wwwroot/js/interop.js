// Interop mínimo para posicionar menus flutuantes (dropdown de prioridade, menu de 3 pontos do
// card de tarefa) fora do fluxo normal — necessário porque esses cards vivem dentro de um
// container com overflow-x:auto (rolagem horizontal do quadro Kanban), e o CSS força o
// overflow-y desse mesmo container para "auto" também (regra do spec: um eixo non-visible força
// o outro a sair de "visible"), o que cria uma barra de rolagem vertical própria da seção e corta
// qualquer menu que extrapole a altura natural do card. Posicionando o menu como position:fixed
// com coordenadas lidas daqui, ele escapa desse recorte inteiramente.
window.jessInterop = {
    getBoundingRect: function (element) {
        if (!element) {
            return null;
        }

        const rect = element.getBoundingClientRect();
        return {
            top: rect.top,
            left: rect.left,
            right: rect.right,
            bottom: rect.bottom,
            width: rect.width,
            height: rect.height,
            viewportWidth: window.innerWidth,
            viewportHeight: window.innerHeight
        };
    },

    // Usado pelo "mostrar mais/menos" da lista de tags (FiltroArquivos.razor): scrollHeight reporta
    // a altura total do conteúdo mesmo quando o elemento tem overflow:hidden + max-height aplicados
    // pra mostrar só as N primeiras linhas — dá pra descobrir quantas linhas o conteúdo *todo* ocupa
    // sem precisar expandir de verdade pra medir.
    getScrollHeight: function (element) {
        return element ? element.scrollHeight : 0;
    }
};
