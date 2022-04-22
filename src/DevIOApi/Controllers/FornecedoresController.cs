using AutoMapper;
using DevIO.Business.Intefaces;
using DevIOApi.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace DevIOApi.Controllers
{
    [Route("api/fornecedores")]
    public class FornecedoresController : MainController
    {
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IMapper _mapper;
        public FornecedoresController(IFornecedorRepository fornecedorRepository, IMapper mapper)
        {
            _fornecedorRepository = fornecedorRepository;
        }

        public async Task<ActionResult<IEnumerable<FornecedorViewModel>>> ObterTodos()
        {
            //var fornecedor = _mapper.Map<IEnumerable< FornecedorViewModel>>(await _fornecedorRepository.ObterTodos());
            var fornecedor = (await _fornecedorRepository.ObterTodos()).Select(f => _mapper.Map<FornecedorViewModel>(f));
            
            return Ok(fornecedor);
        }
    }
}