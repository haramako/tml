require 'sinatra'

get '/:name' do
  name = params[:name]
  out = IO.popen("mono ../Tml/bin/Debug/Tml.exe #{name}"){|f| f.read}
  out
end
