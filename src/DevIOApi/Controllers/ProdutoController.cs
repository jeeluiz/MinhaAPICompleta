using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevIOApi.Controllers
{
    [Authorize]
    [Route("api/fonecedores")]
    public class ProdutoController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IProdutoService _produtoService;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoController(
            IMapper mapper,
            IProdutoRepository produtoRepository,
            INotificador notificador,
            IEnderecoRepository enderecoRepository,
            IUser user) : base(notificador, user)
        {
            _produtoRepository = _produtoRepository;
            _produtoService = _produtoService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos(Guid id)
        {
            return _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores(id));
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null) return NotFound();

            return produtoViewModel;
        }
        
        [HttpGet]
        public async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutosFornecedores(id));
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar([FromBody] ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;
            if (!UploadArquivo(produtoViewModel.Imagem, imagemNome))
            {
                return CustomResponse();
            }

            produtoViewModel.Imagem = imagemNome;
            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            var produto = await ObterProduto(id);

            if (produto == null) return NotFound();

            await _produtoService.Remover(id);

            return CustomResponse(produto);
        }
        private bool UploadArquivo(string arquivo, string imgNome)
        {
            var imageDataByteArray = Convert.FromBase64String(arquivo);
            
            if(string.IsNullOrEmpty(arquivo))
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot/imagens", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Ja existe um arquivo com este nome!");
                return false;
            }
            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);
            return true;
        }
    }
}
