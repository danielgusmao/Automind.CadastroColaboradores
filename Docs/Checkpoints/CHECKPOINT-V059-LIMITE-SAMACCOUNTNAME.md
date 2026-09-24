# CHECKPOINT V059 - CadColab v0.1.2 - limite visual do login sAMAccountName

Data: 24/09/2026.


- mantido o limite tecnico ja existente no backend: `sAMAccountName` com no maximo 20 caracteres;
- campo **Login** passa a exibir permanentemente `max. 20 caracteres no AD`;
- adicionado contador visual `N / 20`;
- ao receber valor acima de 20 caracteres (por exemplo via importacao TOPdesk), a tela destaca o campo e informa quantos caracteres excederam o limite;
- edicao manual do campo usa `maxlength=20`, evitando novos caracteres acima do limite;
- a regra nao trunca silenciosamente um login ja importado: o usuario consegue identificar o excesso e ajustar conscientemente;
- validacao backend continua sendo a autoridade e retorna mensagem explicita com o tamanho recebido e o limite 20;
- motivacao registrada: `teste.provisionamento` excedeu o limite e precisou ser reduzido para `teste.cadcolab`;
- nenhuma regra de OU, grupo, M365, `proxyAddresses`, `pwdLastSet` ou escrita AD foi alterada;
- `Automind:Mode` permanece `ReadOnly`;
- pacote completo versionado como `CadColab-v0.1.2.zip`.

